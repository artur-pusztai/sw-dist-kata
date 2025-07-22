
using SoftwareDistributionKata.Core;
using NUnit.Framework;

namespace SoftwareDistributionKata.Tests;

[TestFixture]
public class GetIntendedPackageTests
{
    private Service _service;
    private const string HostGuid = "host-guid-123";
    private const string ActivationCode = "ABC123";
    private const string CustomerName = "Test Customer";
    private const string AppName = "TestApp";
    private const string Country = "US";
    private const string OtherCountry = "DE";

    private const string OrderNumber = "ORD-001";

    [SetUp]
    public void SetUp()
    {
        _service = new Service();
    }

    [Test]
    public void IfMultiplePackagesClearedButLatestNotRollingOut_ReturnsHighestRolledOutPackageForCountry()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = HostGuid
        };
        _service.AddRegistration(registration);

        var package1 = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        var package2 = new Package(AppName, "2.0.0", rollout: true, clearedCountries: new List<string> { Country });
        var package3 = new Package(AppName, "3.0.0", rollout: false, clearedCountries: new List<string> { Country }); // Not rolled out
        _service.AddPackage(package1);
        _service.AddPackage(package2);
        _service.AddPackage(package3);

        // Act
        var result = _service.GetIntendedPackage(HostGuid);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Version, Is.EqualTo("2.0.0"));
        Assert.That(result.ClearedCountries, Does.Contain(Country));
        Assert.That(result.Rollout, Is.True);
    }

    [Test]
    public void IfMultiplePackagesRollingOutButLatestNotCleared_ReturnsHighestClearedPackageForCountry()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = HostGuid
        };
        _service.AddRegistration(registration);

        var package1 = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        var package2 = new Package(AppName, "2.0.0", rollout: true, clearedCountries: new List<string> { Country });
        var package3 = new Package(AppName, "3.0.0", rollout: true, clearedCountries: new List<string> { OtherCountry }); // Not rolled out
        _service.AddPackage(package1);
        _service.AddPackage(package2);
        _service.AddPackage(package3);

        // Act
        var result = _service.GetIntendedPackage(HostGuid);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Version, Is.EqualTo("2.0.0"));
        Assert.That(result.ClearedCountries, Does.Contain(Country));
        Assert.That(result.Rollout, Is.True);
    }

    [Test]
    public void IfNoPackagesAvailable_Fails()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = HostGuid
        };
        _service.AddRegistration(registration);
        // No packages added for this app

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.GetIntendedPackage(HostGuid)
        );
        Assert.That(ex.Message, Is.EqualTo("No package available for this user"));
    }

    [Test]
    public void IfOurHostNotRegistered_Fails()
    {
        // Arrange
        // Add a registration for a different host (not the one being tested)
        var otherRegistration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = "some-other-host-guid"
        };
        _service.AddRegistration(otherRegistration);
        var package = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        _service.AddPackage(package);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.GetIntendedPackage("unregistered-host-guid")
        );
        Assert.That(ex.Message, Is.EqualTo("Host not registered"));
    }


    [Test]
    public void IfNoPackageClearedForCountryButOrderIsApproved_ReturnsHighestApprovedPackage()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ActivationCode, OrderNumber)
        {
            HostGuid = HostGuid
        };
        _service.AddRegistration(registration);

        // No package is cleared for the registration's country
        var package1 = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { OtherCountry });
        var package2 = new Package(AppName, "2.0.0", rollout: true, clearedCountries: new List<string> { OtherCountry });
        // Simulate that package2 is approved for this order
        package2.ApprovedOrders = new List<string> { OrderNumber };
        _service.AddPackage(package1);
        _service.AddPackage(package2);

        // Act
        var result = _service.GetIntendedPackage(HostGuid);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Version, Is.EqualTo("2.0.0"));
        Assert.That(result.ApprovedOrders, Does.Contain(OrderNumber));
    }
}
