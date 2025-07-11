using NUnit.Framework;

namespace SoftwareDistributionKata
{
    [TestFixture]
    public class SoftwareDistributionServiceTests
    {
        private SoftwareDistributionService service;

        [SetUp]
        public void Setup()
        {
            service = new SoftwareDistributionService();
            InitializeTestData(service);
        }

        [Test]
        public void Register_ValidActivationCode_ReturnsRegistration()
        {
            // Act
            var registration = service.Register("host123", "ABC123");

            // Assert
            Assert.That(registration, Is.Not.Null);
            Assert.That(registration.App, Is.EqualTo("PhotoEditor"));
            Assert.That(registration.InstalledVersion, Is.Null); // Should be null until confirmed
            Assert.That(registration.Country, Is.EqualTo("DE"));
            Assert.That(registration.HostGuid, Is.EqualTo("host123"));
            Assert.That(registration.LastUpdate, Is.LessThanOrEqualTo(DateTime.Now));
        }

        [Test]
        public void Register_InvalidActivationCode_ThrowsException()
        {
            // Act & Assert
            Assert.That(() => service.Register("host123", "INVALID"),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Register_AlreadyUsedActivationCode_ThrowsException()
        {
            // Arrange
            var registration = service.Register("host123", "ABC123");
            var package = service.GetIntendedPackage("host123");
            service.ConfirmInstallation("host123", package);

            // Act & Assert
            Assert.That(() => service.Register("host456", "ABC123"),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("already been used"));
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
            var reg1 = service.Register("host123", "ABC123");

            // Act
            var reg2 = service.Register("host123", "ABC123");

            // Assert
            Assert.That(reg2, Is.SameAs(reg1)); // Should return the same registration object
        }

        [Test]
        public void Register_USACustomer_GetsCorrectPackage()
        {
            // Act
            var registration = service.Register("host456", "XYZ789");

            // Assert
            Assert.That(registration.App, Is.EqualTo("VideoConverter"));
            Assert.That(registration.InstalledVersion, Is.Null); // Should be null until confirmed
            Assert.That(registration.Country, Is.EqualTo("USA"));
            Assert.That(registration.HostGuid, Is.EqualTo("host456"));
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
            Assert.That(result.ClearedCountries, Does.Contain("USA"));
        }

        [Test]
        public void ConfirmInstallation_ValidPackage_UpdatesRegistration()
        {
            // Arrange
            var registration = service.Register("host123", "ABC123");
            var package = service.GetIntendedPackage("host123");

            // Act
            var result = service.ConfirmInstallation("host123", package);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.InstalledVersion, Is.EqualTo(package.Version));
            Assert.That(result.LastUpdate, Is.LessThanOrEqualTo(DateTime.Now));
        }

        [Test]
        public void ConfirmInstallation_InvalidPackage_ThrowsException()
        {
            // Arrange
            service.Register("host123", "ABC123");
            var invalidPackage = new Package("WrongApp", "1.0.0", true, new List<string> { "DE" });

            // Act & Assert
            Assert.That(() => service.ConfirmInstallation("host123", invalidPackage),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not valid"));
        }

        [Test]
        public void ConfirmInstallation_OlderVersion_ThrowsException()
        {
            // Arrange
            service.Register("host123", "DEF456");
            var currentPackage = service.GetIntendedPackage("host123");
            service.ConfirmInstallation("host123", currentPackage);

            var olderPackage = new Package("PhotoEditor", "1.0.0", true, new List<string> { "IN" });

            // Act & Assert
            Assert.That(() => service.ConfirmInstallation("host123", olderPackage),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("older version"));
        }

        [Test]
        public void GetIntendedPackage_NeverReturnsOlderVersion()
        {
            // Arrange - Add a newer version that becomes available later
            service.AddPackage(new Package("PhotoEditor", "1.2.0", true, new List<string> { "IN" }));

            service.Register("host123", "DEF456");
            var package = service.GetIntendedPackage("host123"); // Should get 1.2.0
            Assert.That(package.Version, Is.EqualTo("1.2.0"));

            // Install an older version manually to simulate having an older installed version
            var olderPackage = new Package("PhotoEditor", "1.1.0", true, new List<string> { "IN" });
            service.ConfirmInstallation("host123", olderPackage);

            // Act
            var intendedPackage = service.GetIntendedPackage("host123");

            // Assert
            Assert.That(intendedPackage.Version, Is.EqualTo("1.2.0")); // Should return the newer version, not older
        }

        private void InitializeTestData(SoftwareDistributionService service)
        {
            // Sample packages
            service.AddPackage(new Package("PhotoEditor", "1.0.0", true, new List<string> { "DE", "USA", "IN" }));
            service.AddPackage(new Package("PhotoEditor", "1.1.0", true, new List<string> { "DE", "USA", "IN", "SK" }));
            service.AddPackage(new Package("PhotoEditor", "2.0.0", false, new List<string> { "DE", "USA" }));
            service.AddPackage(new Package("VideoConverter", "1.0.0", true, new List<string> { "DE", "USA", "IN", "SK", "HU", "RO" }));
            service.AddPackage(new Package("VideoConverter", "1.2.0", true, new List<string> { "DE", "USA", "IN" }));
            service.AddPackage(new Package("MusicPlayer", "3.0.0", true, new List<string> { "DE", "USA" }));

            // Sample registrations
            service.AddRegistration(new Registration("ACME Corp", "PhotoEditor", "DE", "ABC123"));
            service.AddRegistration(new Registration("TechStart Inc", "VideoConverter", "USA", "XYZ789"));
            service.AddRegistration(new Registration("GlobalSoft", "PhotoEditor", "IN", "DEF456"));
            service.AddRegistration(new Registration("EuroTech", "MusicPlayer", "SK", "GHI789"));
        }
    }
}