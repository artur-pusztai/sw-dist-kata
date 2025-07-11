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
            var package = service.Register("host123", "ABC123");

            // Assert
            Assert.That(package, Is.Not.Null);
            Assert.That(package.App, Is.EqualTo("PhotoEditor"));
            Assert.That(package.Version, Is.EqualTo("1.1.0"));
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
            Assert.That(secondResult.Version, Is.EqualTo(firstResult.Version));
            Assert.That(secondResult.Version, Is.EqualTo(firstResult.Version));
        }

        [Test]
        public void Register_USACustomer_GetsCorrectPackage()
        {
            // Act
            var package = service.Register("host456", "XYZ789");

            // Assert
            Assert.That(package.App, Is.EqualTo("VideoConverter"));
            Assert.That(package.Version, Is.EqualTo("1.2.0"));
            Assert.That(package.ClearedCountries, Does.Contain("USA"));
            Assert.That(package.Rollout, Is.True);
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
            service.AddRegistration(new Registration("ACME Corp", "PhotoEditor", "DE", "ABC123", "1.0.0"));
            service.AddRegistration(new Registration("TechStart Inc", "VideoConverter", "USA", "XYZ789"));
            service.AddRegistration(new Registration("GlobalSoft", "PhotoEditor", "IN", "DEF456"));
            service.AddRegistration(new Registration("EuroTech", "MusicPlayer", "SK", "GHI789"));
        }
    }
}