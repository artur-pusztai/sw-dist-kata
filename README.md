# Refactoring Kata: Software Distribution

This kata is about refactoring a software distribution system in order to introduce a new feature.

## Requirements Specification

We have a software distribution in place offering public APIs to distribute software to customers.
We can distribute different apps, each app has several versions. Each version of an app we denote as package.

- All `packages` have a `app` - indicating the app name.
- All `packages` have a `version`, using semantic versioning. The `app` and the `version` pair is unique across all packages.
- All `packages` have a `rollout` flag, indicating they are rolling out to all customers entitled to use it.
- All `packages` have a list of `clearedCountries` (DE, USA, IN, SK, HU, RO, ...) indicating countries allowed to
  install this version of this app.

Customers receive an activation code, an one-time-password they can use to download and install the latest app
for their location. We keep track of apps sold, installed and updated by customers using a registration record.

- All `registrations` have a `customer` - the name of the customer
- All `registrations` have an `app` - the name of the app. The combination of `customer` and `app` is unique across all registrations.
- All `registrations` have one `country`
- All `registrations` have an `activationCode`
- All `registrations` have an `installedVersion`
- All `registrations` have a `hostGuid` - the unique identifier of the host where the app is installed.
- All `registrations` have a `lastUpdate` - the date when the registration was last updated.
- All `registrations` have a `order` field - the sales order which maps back to the sales system.

This is the API for the customer:

- `public Registration Register(HostGuid, ActivationCode)` - the client registers and receives his registration record
- `public Package GetIntendedPackage(HostGuid)` - the client gets the package he is intended to install.
- `public Registration ConfirmInstallation(HostGuid, Package)` - the client confirms the installation of a package and
  receives the updated registration record

The GetIntendedPackage must never return a package older that the customer has currently installed.

An Activation Code is valid until the installation of the first package is confirmed.
After that, the activation code is invalidated.

Packages are uploaded to the system using a web interface, not part of this exercise.

There is a sync with the sales database every 24 hours, which extends the registrations with the newly sold apps.

There is another sync with Regulatory Affairs every 24 hours, which updates the packages with the latest cleared countries.

## Refactoring Tasks

Improve the tests to such extent that you can confidently start refactoring the code.
Refactor the code to feel comfortable adding the following feature

```shell
# Run tests with coverage
dotnet test SoftwareDistributionKata.sln --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:"SoftwareDistributionKata.Tests\TestResults\*\coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html

# View the report
# Open CoverageReport\index.html in a browser
```

## Additional Features

1. Sales arranged with Regulatory Affairs that some packages can be delivered
   to some countries before the country is cleared.  
   The condition is complex, but foreign affairs can provide for each package a list of approved orders in addition to
   the cleared countries.  
   They require to include the cleared orders in the logic of choosing the intended package.  
   *Example:* Customer A has a registration for Viewer 1.0 in DE, but Viewer 1.0 has no clearing for DE yet.
   However, the order ID of the customer appears in the list of approved orders for Viewer 1.0,
   so it can be delivered to the customer.
2. Some new Apps are planned, which have dependencies to each other.
   The dependency will be part of the package definition.  
   **The distribution system must ensure that the dependencies are met when choosing the intended package.**  
   *Example:* Viewer 1.1 depends on Browser and is compatible with Browser 1.0 and 1.1.  
   In this case, the same customer can not install Viewer 1.0 until he has installed Browser 1.0 or Browser 1.1.
