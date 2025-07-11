using NUnit.Framework;

namespace SoftwareDistributionKata
{
    public class Package
    {
        public string App { get; set; }
        public string Version { get; set; }
        public bool Rollout { get; set; }
        public List<string> ClearedCountries { get; set; }

        public Package(string app, string version, bool rollout, List<string> clearedCountries)
        {
            App = app;
            Version = version;
            Rollout = rollout;
            ClearedCountries = clearedCountries ?? new List<string>();
        }
    }

    public class Registration
    {
        public string Customer { get; set; }
        public string App { get; set; }
        public string Country { get; set; }
        public string ActivationCode { get; set; }
        public string InstalledVersion { get; set; }
        public string HostGuid { get; set; }
        public DateTime LastUpdate { get; set; }

        public Registration(string customer, string app, string country, string activationCode, string installedVersion = null)
        {
            Customer = customer;
            App = app;
            Country = country;
            ActivationCode = activationCode;
            InstalledVersion = installedVersion;
            LastUpdate = DateTime.Now;
        }
    }

    public class SoftwareDistributionService
    {
        private List<Package> packages;
        private List<Registration> registrations;

        public SoftwareDistributionService()
        {
            packages = new List<Package>();
            registrations = new List<Registration>();
        }

        public Registration Register(string hostGuid, string activationCode)
        {
            // Find the registration by activation code
            Registration registration = null;
            foreach (var reg in registrations)
            {
                if (reg.ActivationCode == activationCode)
                {
                    registration = reg;
                    break;
                }
            }

            if (registration == null)
            {
                throw new InvalidOperationException("Invalid activation code");
            }

            // Check if activation code is still valid (no package has been installed yet)
            if (!string.IsNullOrEmpty(registration.InstalledVersion))
            {
                throw new InvalidOperationException("Activation code has already been used");
            }

            // Set the host GUID and update timestamp
            registration.HostGuid = hostGuid;
            registration.LastUpdate = DateTime.Now;

            return registration;
        }

        public Package GetIntendedPackage(string hostGuid)
        {
            // Find registration by hostGuid
            Registration userRegistration = null;
            foreach (var reg in registrations)
            {
                if (reg.HostGuid == hostGuid)
                {
                    userRegistration = reg;
                    break;
                }
            }

            if (userRegistration == null)
            {
                throw new InvalidOperationException("Host not registered");
            }

            // Find the latest available package for this app and country
            Package intendedPackage = null;
            string highestVersion = userRegistration.InstalledVersion ?? "0.0.0";

            foreach (var package in packages)
            {
                if (package.App == userRegistration.App &&
                    package.Rollout &&
                    package.ClearedCountries.Contains(userRegistration.Country))
                {
                    // Simple version comparison (assumes semantic versioning)
                    if (IsVersionNewerOrSame(package.Version, highestVersion))
                    {
                        intendedPackage = package;
                        highestVersion = package.Version;
                    }
                }
            }

            if (intendedPackage == null)
            {
                throw new InvalidOperationException("No package available for this user");
            }

            return intendedPackage;
        }

        public Registration ConfirmInstallation(string hostGuid, Package package)
        {
            // Find registration by hostGuid
            Registration userRegistration = null;
            foreach (var reg in registrations)
            {
                if (reg.HostGuid == hostGuid)
                {
                    userRegistration = reg;
                    break;
                }
            }

            if (userRegistration == null)
            {
                throw new InvalidOperationException("Host not registered");
            }

            // Verify the package is valid for this user
            if (package.App != userRegistration.App ||
                !package.Rollout ||
                !package.ClearedCountries.Contains(userRegistration.Country))
            {
                throw new InvalidOperationException("Package is not valid for this user");
            }

            // Update the registration
            userRegistration.InstalledVersion = package.Version;
            userRegistration.LastUpdate = DateTime.Now;

            return userRegistration;
        }

        private bool IsVersionNewerOrSame(string version1, string version2)
        {
            var version1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var version2Parts = version2.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Min(version1Parts.Length, version2Parts.Length); i++)
            {
                if (version1Parts[i] >= version2Parts[i])
                {
                    return true;
                }
                else if (version1Parts[i] < version2Parts[i])
                {
                    return false;
                }
            }

            return version1Parts.Length >= version2Parts.Length;
        }

        internal void AddPackage(Package package)
        {
            packages.Add(package);
        }

        internal void UpdatePackage(Package package)
        {
            // Find the existing package to update
            var existingPackage = packages.FirstOrDefault(p => p.App == package.App && p.Version == package.Version);
            if (existingPackage != null)
            {
                // Update the existing package
                existingPackage.Rollout = package.Rollout;
                existingPackage.ClearedCountries = package.ClearedCountries;
            }
        }

        public void AddRegistration(Registration registration)
        {
            registrations.Add(registration);
        }
    }
}