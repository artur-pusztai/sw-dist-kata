using System.Runtime.CompilerServices;
// Allow test project access to internals
[assembly: InternalsVisibleTo("SoftwareDistributionKata.Tests")]
namespace SoftwareDistributionKata.Core
{
    public class Service
    {
        private readonly List<Package> packages;
        private readonly List<Registration> registrations;

        public Service()
        {
            packages = [];
            registrations = [];
        }

        public Registration Register(string hostGuid, string activationCode)
        {
            Registration registration = registrations.FirstOrDefault(reg => reg.ActivationCode == activationCode)
                ?? throw new InvalidOperationException("Invalid activation code");
            if (IsInstalled(registration))
                throw new InvalidOperationException("Activation code has already been used");
            registration.HostGuid = hostGuid;
            registration.LastUpdate = DateTime.Now;
            return registration;
        }

        private static bool IsInstalled(Registration registration)
        {
            return !string.IsNullOrEmpty(registration.InstalledVersion);
        }

        public Package GetIntendedPackage(string hostGuid)
        {
            // Find registration by hostGuid
            Registration? userRegistration = registrations.FirstOrDefault(reg => reg.HostGuid == hostGuid) ?? throw new InvalidOperationException("Host not registered");

            // Find the latest available package for this app and country
            Package? intendedPackage = null;
            var allowedPackages = packages.Where(package => IsUserAllowedToInstallPackage(userRegistration, package)).ToList();
            string highestVersion = userRegistration.InstalledVersion ?? "0.0.0";
            foreach (var package in allowedPackages)
            {
                if (IsVersionNewerOrSame(package.Version, highestVersion))
                {
                    intendedPackage = package;
                    highestVersion = package.Version;
                }
            }

            return intendedPackage ?? throw new InvalidOperationException("No package available for this user");
        }

        private static bool IsUserAllowedToInstallPackage(Registration userRegistration, Package package)
        {
            return package.App == userRegistration.App &&
                   package.Rollout &&
                   package.ClearedCountries.Contains(userRegistration.Country);
        }

        public Registration ConfirmInstallation(string hostGuid, Package package)
        {
            // Find registration by hostGuid
            Registration? userRegistration = registrations.FirstOrDefault(reg => reg.HostGuid == hostGuid) ?? throw new InvalidOperationException("Host not registered");

            // Verify the package is valid for this user
            if (!IsUserAllowedToInstallPackage(userRegistration, package))
            {
                throw new InvalidOperationException("Package is not valid for this user");
            }

            // Update the registration
            userRegistration.InstalledVersion = package.Version;
            userRegistration.LastUpdate = DateTime.Now;

            return userRegistration;
        }

        internal static bool IsVersionNewerOrSame(string version1, string version2)
        {
            var version1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var version2Parts = version2.Split('.').Select(int.Parse).ToArray();
            for (int i = 0; i < Math.Min(version1Parts.Length, version2Parts.Length); i++)
            {
                if (version1Parts[i] > version2Parts[i]) return true;
                if (version1Parts[i] < version2Parts[i]) return false;
            }
            return version1Parts.Length >= version2Parts.Length;
        }

        public void AddPackage(Package package)
        {
            packages.Add(package);
        }

        public void UpdatePackage(Package package)
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
