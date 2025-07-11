using System;
using System.Collections.Generic;
using System.Linq;
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
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            // Sample packages
            packages.Add(new Package("PhotoEditor", "1.0.0", true, new List<string> { "DE", "USA", "IN" }));
            packages.Add(new Package("PhotoEditor", "1.1.0", true, new List<string> { "DE", "USA", "IN", "SK" }));
            packages.Add(new Package("PhotoEditor", "2.0.0", false, new List<string> { "DE", "USA" }));
            packages.Add(new Package("VideoConverter", "1.0.0", true, new List<string> { "DE", "USA", "IN", "SK", "HU", "RO" }));
            packages.Add(new Package("VideoConverter", "1.2.0", true, new List<string> { "DE", "USA", "IN" }));
            packages.Add(new Package("MusicPlayer", "3.0.0", true, new List<string> { "DE", "USA" }));

            // Sample registrations
            registrations.Add(new Registration("ACME Corp", "PhotoEditor", "DE", "ABC123", "1.0.0"));
            registrations.Add(new Registration("TechStart Inc", "VideoConverter", "USA", "XYZ789"));
            registrations.Add(new Registration("GlobalSoft", "PhotoEditor", "IN", "DEF456"));
            registrations.Add(new Registration("EuroTech", "MusicPlayer", "SK", "GHI789"));
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

            // Check if already registered for this host
            foreach (var reg in registrations)
            {
                if (reg.ActivationCode == activationCode && !string.IsNullOrEmpty(reg.InstalledVersion))
                {
                    reg.HostGuid = hostGuid; // Update host GUID if not set
                    return reg;
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
    }

    [TestFixture]
    public class SoftwareDistributionServiceTests
    {
        private SoftwareDistributionService service;

        [SetUp]
        public void Setup()
        {
            service = new SoftwareDistributionService();
        }

        [Test]
        public void Register_ValidActivationCode_ReturnsRegistration()
        {
            // Act
            var result = service.Register("host123", "ABC123");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Customer, Is.EqualTo("ACME Corp"));
            Assert.That(result.App, Is.EqualTo("PhotoEditor"));
            Assert.That(result.Country, Is.EqualTo("DE"));
            Assert.That(result.ActivationCode, Is.EqualTo("ABC123"));
            Assert.That(result.InstalledVersion, Is.Not.Null);
        }

        [Test]
        public void Register_InvalidActivationCode_ThrowsException()
        {
            // Act & Assert
            Assert.That(() => service.Register("host123", "INVALID"),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Register_CountryNotCleared_ThrowsException()
        {
            // Act & Assert
            Assert.That(() => service.Register("host123", "GHI789"),
                Throws.TypeOf<InvalidOperationException>()); // MusicPlayer not available in SK
        }

        [Test]
        public void GetIntendedPackage_RegisteredHost_ReturnsLatestPackage()
        {
            // Arrange
            service.Register("host123", "ABC123");

            // Act
            var result = service.GetIntendedPackage("host123");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.App, Is.EqualTo("PhotoEditor"));
            Assert.That(result.Version, Is.EqualTo("1.1.0")); // Should be latest available version
            Assert.That(result.Rollout, Is.True);
            Assert.That(result.ClearedCountries, Contains.Item("DE"));
        }

        [Test]
        public void GetIntendedPackage_UnregisteredHost_ThrowsException()
        {
            // Act & Assert
            Assert.That(() => service.GetIntendedPackage("unregistered-host"),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Register_AlreadyRegistered_ReturnsSameRegistration()
        {
            // Arrange
            var firstResult = service.Register("host123", "ABC123");

            // Act
            var secondResult = service.Register("host123", "ABC123");

            // Assert
            Assert.That(secondResult.InstalledVersion, Is.EqualTo(firstResult.InstalledVersion));
            Assert.That(secondResult.Customer, Is.EqualTo(firstResult.Customer));
        }

        [Test]
        public void Register_USACustomer_GetsCorrectPackage()
        {
            // Act
            var result = service.Register("host456", "XYZ789");

            // Assert
            Assert.That(result.Customer, Is.EqualTo("TechStart Inc"));
            Assert.That(result.App, Is.EqualTo("VideoConverter"));
            Assert.That(result.Country, Is.EqualTo("USA"));
            Assert.That(result.InstalledVersion, Is.EqualTo("1.2.0")); // Latest available for USA
        }

        [Test]
        public void GetIntendedPackage_AfterRegistration_ReturnsCorrectPackage()
        {
            // Arrange
            service.Register("host456", "XYZ789");

            // Act
            var result = service.GetIntendedPackage("host456");

            // Assert
            Assert.That(result.App, Is.EqualTo("VideoConverter"));
            Assert.That(result.Version, Is.EqualTo("1.2.0"));
            Assert.That(result.ClearedCountries.Contains("USA"), Is.True);
        }
    }
}
