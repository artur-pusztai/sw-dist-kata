using NUnit.Framework;
using SoftwareDistributionKata.Core;

namespace SoftwareDistributionKata.Tests;

[TestFixture]
public class ConfirmInstallationTests
{
    private Service _service;
    private const string HostGuid = "host-guid-123";
    private const string ActivationCode = "ABC123";
    private const string CustomerName = "Test Customer";
    private const string AppName = "TestApp";
    private const string Country = "US";
    private const string OrderNumber = "ORD-001";

    [SetUp]
    public void SetUp()
    {
        _service = new Service();
    }

    [Test]
    public void WhenRegistrationFoundAndUserEntitled_Succeeds()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = HostGuid
        };
        _service.AddRegistration(registration);

        var package = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        _service.AddPackage(package);

        // Act
        var result = _service.ConfirmInstallation(HostGuid, package);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.InstalledVersion, Is.EqualTo("1.0.0"));
        Assert.That(result.LastUpdate, Is.Not.Null);
        Assert.That(result.HostGuid, Is.EqualTo(HostGuid));
    }

    [Test]
    public void WhenPackageNotRolledOut_Fails()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = HostGuid
        };
        _service.AddRegistration(registration);

        var package = new Package(AppName, "1.0.0", rollout: false, clearedCountries: new List<string> { Country });
        _service.AddPackage(package);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.ConfirmInstallation(HostGuid, package)
        );
        Assert.That(ex.Message, Is.EqualTo("Package is not valid for this user"));
    }

    [Test]
    public void WhenCountryNotCleared_Fails()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = HostGuid
        };
        _service.AddRegistration(registration);

        var package = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { "DE" });
        _service.AddPackage(package);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.ConfirmInstallation(HostGuid, package)
        );
        Assert.That(ex.Message, Is.EqualTo("Package is not valid for this user"));
    }

    [Test]
    public void WhenRegistrationNotFound_Fails()
    {
        // Arrange
        var package = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        _service.AddPackage(package);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.ConfirmInstallation(HostGuid, package)
        );
        Assert.That(ex.Message, Is.EqualTo("Host not registered"));
    }

}
