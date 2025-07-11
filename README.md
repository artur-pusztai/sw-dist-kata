# Requirements Specification

We have a software distribution in place offering public APIs to distribute software to customers. We can distribute different apps, each app has several versions. Each version of an app we denote as package.

- All `packages` have a `app` - indicating the app name.
- All `packages` have a `version`, using semantic versioning. The `app` and the `version` pair is unique across all packages.
- All `packages` have a `rollout` flag, indicating they are rolling out to all customers entitled to use it.
- All `packages` have a list of `clearedCountries` (DE, USA, IN, SK, HU, RO, ...) indicating countries allowed to install this version of this app.

Customers receive an activation code, an one-time-password they can use to download and install the latest app for their location. We keep track of apps sold, installed and updated by customers using a registration record.

- All `registrations` have a `customer` - the name of the customer
- All `registrations` have an `app` - the name of the app. The combination of `customer` and `app` is unique across all registrations.
- All `registrations` have one `country`
- All `registrations` have an `activationCode` 
- All `registrations` have an `installedVersion` 

This is the API for the customer:

- `public Registration Register(HostGuid, ActivationCode)` - the client registers and receives his registration record
- `public Package GetIntendedPackage(HostGuid)` - the client gets the package he is intended to install.

