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

        public Registration(string customer, string app, string country, string activationCode, string installedVersion = null)
        {
            Customer = customer;
            App = app;
            Country = country;
            ActivationCode = activationCode;
            InstalledVersion = installedVersion;
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

        public Package Register(string hostGuid, string activationCode)
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

            // Check if already registered for this host
            foreach (var reg in registrations)
            {
                if (reg.ActivationCode == activationCode && !string.IsNullOrEmpty(reg.InstalledVersion))
                {
                    reg.HostGuid = hostGuid; // Update host GUID if not set
                    return GetIntendedPackage(hostGuid);
                }
            }

            // Find the latest available package for this app and country
            Package latestPackage = null;
            string highestVersion = "0.0.0";

            foreach (var package in packages)
            {
                if (package.App == registration.App &&
                    package.Rollout &&
                    package.ClearedCountries.Contains(registration.Country))
                {
                    // Simple version comparison (assumes semantic versioning)
                    var currentVersionParts = package.Version.Split('.').Select(int.Parse).ToArray();
                    var highestVersionParts = highestVersion.Split('.').Select(int.Parse).ToArray();

                    bool isNewer = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (currentVersionParts[i] > highestVersionParts[i])
                        {
                            isNewer = true;
                            break;
                        }
                        else if (currentVersionParts[i] < highestVersionParts[i])
                        {
                            break;
                        }
                    }

                    if (isNewer)
                    {
                        latestPackage = package;
                        highestVersion = package.Version;
                    }
                }
            }

            if (latestPackage == null)
            {
                throw new InvalidOperationException("No package available for this country");
            }

            // Update the registration with the installed version and host GUID
            registration.InstalledVersion = latestPackage.Version;
            registration.HostGuid = hostGuid;

            return GetIntendedPackage(hostGuid);
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
            string highestVersion = "0.0.0";

            foreach (var package in packages)
            {
                if (package.App == userRegistration.App &&
                    package.Rollout &&
                    package.ClearedCountries.Contains(userRegistration.Country))
                {
                    // Simple version comparison (assumes semantic versioning)
                    var currentVersionParts = package.Version.Split('.').Select(int.Parse).ToArray();
                    var highestVersionParts = highestVersion.Split('.').Select(int.Parse).ToArray();

                    bool isNewer = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (currentVersionParts[i] > highestVersionParts[i])
                        {
                            isNewer = true;
                            break;
                        }
                        else if (currentVersionParts[i] < highestVersionParts[i])
                        {
                            break;
                        }
                    }

                    if (isNewer)
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

        internal void AddPackage(Package package)
        {
            packages.Add(package);
        }

        public void AddRegistration(Registration registration)
        {
            registrations.Add(registration);
        }

    }
}