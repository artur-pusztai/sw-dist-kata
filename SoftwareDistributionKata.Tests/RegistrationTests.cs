using SoftwareDistributionKata.Core;

namespace SoftwareDistributionKata.Tests;

[TestFixture]
public class RegistrationTests
{
    private Service _service;
    private const string ValidActivationCode = "ABC123";
    private const string ValidHostGuid = "host-guid-123";
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
    public void WhenActivationCodeIsValidAndPackageIsClearedForCountry_Succeeds()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ValidActivationCode, OrderNumber);
        _service.AddRegistration(registration);

        var package = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        _service.AddPackage(package);

        // Act
        var result = _service.Register(ValidHostGuid, ValidActivationCode);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.HostGuid, Is.EqualTo(ValidHostGuid));
        Assert.That(result.Customer, Is.EqualTo(CustomerName));
        Assert.That(result.App, Is.EqualTo(AppName));
        Assert.That(result.Country, Is.EqualTo(Country));
        Assert.That(result.ActivationCode, Is.EqualTo(ValidActivationCode));
        Assert.That(result.LastUpdate, Is.Not.Null);
        Assert.That(result.InstalledVersion, Is.Null); // Should still be null after registration
    }

    [Test]
    public void WhenActivationCodeIsInvalid_Fails()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ValidActivationCode, OrderNumber);
        _service.AddRegistration(registration);

        var package = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        _service.AddPackage(package);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.Register(ValidHostGuid, "INVALID_CODE")
        );
        Assert.That(ex.Message, Is.EqualTo("Invalid activation code"));
    }

    [Test]
    public void WhenActivationCodeAlreadyUsed_Fails()
    {
        // Arrange
        var registration = new Registration(CustomerName, AppName, Country, ValidActivationCode, OrderNumber)
        {
            InstalledVersion = "1.0.0" // Simulate already installed
        };
        _service.AddRegistration(registration);

        var package = new Package(AppName, "1.0.0", rollout: true, clearedCountries: new List<string> { Country });
        _service.AddPackage(package);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.Register(ValidHostGuid, ValidActivationCode)
        );
        Assert.That(ex.Message, Is.EqualTo("Activation code has already been used"));
    }

}