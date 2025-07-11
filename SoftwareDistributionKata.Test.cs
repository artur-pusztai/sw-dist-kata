using System.Security.Cryptography;
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
        }

        [Test]
        public void Test_UpdateWorkflow()
        {
            // upload a new package for App1
            service.AddPackage(new Package("App1", "1.0.0", true, new List<string> { }));
            // prepare an activation code for customer1
            service.AddRegistration(new Registration("Customer1", "App1", "DE", "ACT123", "Order1"));

            // register customer1 with the activation code must fail as DE is not cleared
            var reg1 = service.Register("host123", "ACT123");

            Assert.That(reg1, Is.Not.Null);
            Assert.That(reg1.App, Is.EqualTo("App1"));
            Assert.That(reg1.Country, Is.EqualTo("DE"));
            Assert.That(reg1.InstalledVersion, Is.Null);

            // GetIntendedPackage should throw an exception as DE is not cleared
            Assert.Throws<InvalidOperationException>(() => service.GetIntendedPackage("host123"));

            // clear DE for App1
            service.UpdatePackage(new Package("App1", "1.0.0", true, new List<string> { "DE" }));

            // GetIntendedPackage should now return the package
            var package = service.GetIntendedPackage("host123");
            Assert.That(package, Is.Not.Null);
            Assert.That(package.App, Is.EqualTo("App1"));
            Assert.That(package.Version, Is.EqualTo("1.0.0"));
            Assert.That(package.ClearedCountries, Contains.Item("DE"));

            // Confirm installation
            var confirmedReg = service.ConfirmInstallation("host123", package);
            Assert.That(confirmedReg, Is.Not.Null);
            Assert.That(confirmedReg.InstalledVersion, Is.EqualTo("1.0.0"));
            Assert.That(confirmedReg.LastUpdate, Is.LessThanOrEqualTo(DateTime.Now));

            // Registering again with the same activation code should fail
            Assert.Throws<InvalidOperationException>(() => service.Register("host123", "ACT123"));

            // upload a new version of App1
            service.AddPackage(new Package("App1", "1.1.0", true, new List<string> { }));

            // GetIntendedPackage should still find same version as new one is not cleared for DE
            var intendedPackage = service.GetIntendedPackage("host123");
            Assert.That(intendedPackage.Version, Is.EqualTo("1.0.0"));
            Assert.That(intendedPackage.ClearedCountries, Contains.Item("DE"));

            // Update the package to include the new version
            service.UpdatePackage(new Package("App1", "1.1.0", true, new List<string> { "DE" }));
            // GetIntendedPackage should now return the new version
            var newPackage = service.GetIntendedPackage("host123");
            Assert.That(newPackage.Version, Is.EqualTo("1.1.0"));
            Assert.That(newPackage.ClearedCountries, Contains.Item("DE"));

            // Confirm installation of the new version
            var newConfirmedReg = service.ConfirmInstallation("host123", newPackage);
            Assert.That(newConfirmedReg, Is.Not.Null);
            Assert.That(newConfirmedReg.InstalledVersion, Is.EqualTo("1.1.0"));
            Assert.That(newConfirmedReg.LastUpdate, Is.LessThanOrEqualTo(DateTime.Now));

            // Remove clearing for DE for already installed package
            service.UpdatePackage(new Package("App1", "1.1.0", true, new List<string> { }));
            // GetIntendedPackage shall now return the latest cleared package
            var pkg = service.GetIntendedPackage("host123");
            Assert.That(pkg.Version, Is.EqualTo("1.0.0"));
        }
    }
}