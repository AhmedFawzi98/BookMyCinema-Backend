# Architecture Testing

The repository includes a dedicated architecture test project: `BookMyCinema.Architecture.Tests`.
The project uses `ArchUnitNET` pacakge.

## Why This Exists

Its purpose is to enforce architectural decisions and automate them as actual tests that can be ran and verified and have feedback when any rule is broken.
They make the intended design rules executable and easy verified, which is more reliable than relying on convention alone.


## What The Tests Protect

The tests focus on a few classes of rules to be enforced:

- layer boundaries 
- namespace placement
- naming conventions
- Visibility 
- Dependency
- Colocation

## Shared Test Setup

`BaseTest` is the shared composition root for the architecture test suite. It loads the assemblies for all solution layers:

- `Domain`
- `Application`
- `Api`
- `Infrastructure`
- `Persistance`
- `WebApp`

It also centralizes:

- the loaded `Architecture` model
- the namespace helper used by the rules
- the list of forbidden namespaces

That setup keeps the individual test classes small and consistent.

## Test Groups

The architecture suite is split by responsibility.

`LayersTests` enforces the high-level dependency rules between layers. These tests protect the direction of dependencies and make sure outer-layer concerns do not leak into the core layers.

The remaining test classes are organized per layer:

- `DomainTests`
- `ApplicationTests`
- `ApiTests`
- `InfrastructureTests`
- `PersistanceTests`

Each layer-specific suite enforces the rules that belong to that layer, such as:

- colocation rules
- visibility rules
- dependency restrictions
- naming conventions

This separation matters because each layer has different constraints.

## Examples Of What Is Checked

The current rules cover patterns such as:

- domain and application layers not depending on `Microsoft.EntityFrameworkCore`
- application code not depending directly on hosting `Microsoft.AspNetCore`
- only things marked public (such as API endpoints implementing `IEndpoint`, or things marked with PubliclyVisibile attribute) should be public, otherwise should be internal to its layer.
- endpoint-related types following the expected naming and colocation conventions
- persistence code staying within persistence-specific namespaces
- validators reside in features folder
