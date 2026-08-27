# BookMyCinema Architecture

BookMyCinema follows a domain-centric Architecture. specifically Clean Architecture.
This document explains how the solution is structured, what responsibilities each layer owns, and how each layer strucutre folders and placement of files.

## Why Clean Architecture

The primary goal of this architecture is to **protect the core business logic by enforcing clear boundaries and directing dependencies inward**, keeping business rules independent of frameworks, persistence, external integrations, and delivery mechanisms while allowing those outer concerns to evolve or be replaced with minimal impact on the core.

- **Business rules remain isolated from frameworks and infrastructure**, keeping the core domain focused on business behavior rather than technical concerns.
- **Infrastructure and persistence are implementation details**, accessed through abstractions where appropriate so they can evolve without affecting core business logic.
- **Use cases are organized by feature and kept self-contained where practical**, making related behavior easy to locate, understand, modify, and test.
- **Dependencies flow in a deliberate direction**, preventing infrastructure and presentation concerns from leaking into the application and domain layers.
- **Expected failures are represented explicitly through results**, while exceptions are reserved for exceptional or unexpected failures. See [Results Pattern](../error-handling/results-pattern.md).
- **Architectural boundaries are enforceable and testable**, allowing automated tests to detect invalid dependencies and protect the intended structure as the codebase evolves. See [Architecture Testing](../testing/architecture-testing.md).

## Solution overview

The solution currently contains the following assemblies:
- `BookMyCinema.SharedKernel`
- `BookMyCinema.Domain`
- `BookMyCinema.Application`
- `BookMyCinema.Api`
- `BookMyCinema.Persistance`
- `BookMyCinema.Infrastructure`
- `BookMyCinema.WebApp`

And these test projects:

- `BookMyCinema.Architecture.Tests`
- `BookMyCinema.Application.UnitTest`
- `BookMyCinema.Api.IntegrationTests`

The dependency direction is one way: outer layers depend on inner layers, while the core inner layers (inward).

`SharedKernel <- Domain <- Application <- Api andInfrastructure and Persistence`

`WebApp` is the host and composition root and is the only project that references all assemblies.


The current layout is trying to preserve a few practical rules:

- business rules should be the core, hence domain layer should not depend on any technology or external dependency or framework.
- outer layers should implement inner-layer contracts instead of pushing technical dependencies inward
- Screaming Architecture: feature code should be easy to find by use case, not scattered across horizontal folders by file type or by technical concerns 
- expected failures should be explicit through results pattern and error model rather than exception-heavy control flow
- persistence concerns are separated from other infrastrucutre (messaging, external systems, etc.) and live in a dedicated assembly.
- the API layer should stay thin and focused on transport and public exposure and HTTP concerns


## Architecture Enforcement

- Clean Architecture decisions are not merely documented conventions; they are codified and continuously enforced through architecture tests.

- supporting docs, especially:
  - [DDD Overview](../ddd/overview.md)
  - [Logical Database Schema](../persistence/logical-schema.md)
  - [Architecture Testing](../testing/architecture-testing.md)


## Project reference map

- `BookMyCinema.SharedKernel`
  - no project references
- `BookMyCinema.Domain`
  - references `BookMyCinema.SharedKernel`
- `BookMyCinema.Application`
  - references `BookMyCinema.Domain`
- `BookMyCinema.Api`
  - references `BookMyCinema.Application`
- `BookMyCinema.Persistance`
  - references `BookMyCinema.Application`
- `BookMyCinema.Infrastructure`
  - references `BookMyCinema.Application`
- `BookMyCinema.WebApp`
  - references `BookMyCinema.Domain`
  - references `BookMyCinema.Application`
  - references `BookMyCinema.Api`
  - references `BookMyCinema.Persistance`
  - references `BookMyCinema.Infrastructure`

## SharedKernel

`BookMyCinema.SharedKernel` is the innermost shared project.
It contains domainc focused building blocks that are reusable across different domains and BCs. It is not shared application services or helpers. It is domain focused.

SharedKernel owns cross-cutting model primitives that are safe to reuse broadly:

- base entity and aggregate root types
- audit capability interfaces:
  - `ICreationAuditable`
  - `IModificationAuditable`
  - `IActivationAuditable`
- domain event abstraction: `IDomainEvent`
- result and error primitives:
  - `Result`
  - `Result<T>`
  - `IResult`
  - `Error`
  - `ErrorKind`
- reusable value objects:
  - `Email`
  - `Country`
  - `Currency`

### What belongs here

- genuinely shared domain primitives, abstractions and shared types that multiple domain areas can depend on without introducing a business-specific dependency
- The Shared Kernel should remain focused and minimal, rather than becoming a dumping ground for broadly reusable code

### Current patterns worth following

Shared value objects currently keep their own creation logic and error catalog close together in the same file, for example:

- `ValueObjects/Email.cs`
- `ValueObjects/Country.cs`

This is intentionally different from Application validation, where request-shape validation errors stay near the use case rather than being promoted into shared code.

`Result`, `Result<T>`, `Error`, and `ErrorKind` also live here because they are used across layers as the repository's expected-failure model. See [Results Pattern](../error-handling/results-pattern.md).

## Domain

`BookMyCinema.Domain` contains domain-specific business concepts.

Domain concepts are expected to be grouped: by domain area, not by technical artifact type.
Where each Bounded Context focus on its domain models, and each domain model groups its entities, value objects, domain events, etc.
So keeping Bounded context and aggregate specific artifacts close together

### What belongs here

- aggregates and aggregate roots
- domain entities and domain-specific value objects
- domain services when needed
- domain events
- domain error catalogs such as `UserErrors`
- enums that represent domain meaning 


## Application

`BookMyCinema.Application` coordinates use cases. It decides how domain rules, validation, repositories, external services, messaging, and other application-facing abstractions are combined to execute a use case.

Application contracts should be named by domain and data shape first, not by technical artifact labels. The convention is to avoid the `Dto` suffix across application DTOs and instead use representative names when possible, so names describe data shape instead of leaking technical concerns.

Commands and queries should use immutable record-based message contracts by default whenever possible. `sealed record` is the general default, and `readonly record struct` may also be used when all fields are value types as a small optimization to avoid heap allocation. Use a `class` only when a record is not suitable for the specific requirement.

### Feature-first organization

Application layer is feature-first organization, each feature use cases are grouped under the feature, and each use case groups its related artifacts (commands/queries DTOs, results DTOs, validators, validations errors, use case handlers, readers/writers services, etc.).
While other `common` abstractions, helpers, extensions, types are kept together separately.

Within that feature-first and use case structure, the repository uses logical CQRS: commands and queries are separated by responsibility and modeling style inside the same application and codebase and database. See [CQRS](./cqrs.md).

### What can be shared at feature level

If something is reused by multiple use cases within the same feature, it should usually move up one level under the feature rather than all the way into `Application/Common`.

Examples of the kinds of things that would belong at feature level when genuinely shared:

- feature-specific DTOs used by several use cases
- feature-specific helper services
- feature-specific constants
- feature-specific read models reused by several queries in the same feature

The current codebase does not yet have a strong implemented sample of this, so treat it as an established convention implied by the feature-first structure rather than a fully demonstrated runtime pattern.

### Domain-model path vs direct persistence/query path

The architecture does not require every operation to pass through the Domain model.
The appropriate path depends on whether the operation needs **domain behavior, invariants, or aggregate state to make a correct business decision**.

**Domain-model path:** Use the Domain model when an operation—read or write—requires an aggregate's complete business state and behavior to evaluate invariants, enforce consistency, validate state transitions, or make domain decisions. In these cases, aggregate-oriented domain repositories load and persist the Domain model as required. Repositories exist to support aggregate boundaries and domain behavior, not as CRUD abstractions for every persisted entity.

**Direct persistence/query path:** Operations may bypass the Domain model when no domain behavior or invariant enforcement is required. This applies both when no corresponding Domain entity exists at all, such as persistence-oriented reference data(Lookups), or even when a Domain model does exist but materializing it provides no value for the operation. Read-heavy or performance-sensitive queries, for example, can project directly from persistence into Application-owned DTOs/read models rather than loading aggregates.

This means persistence and Domain models are intentionally **not required to have a 1:1 relationship**, and the existence of a Domain model does not require every operation over its data to use it.

When database access is required directly by an Application use case, **Application owns the required abstraction and result model, while Persistence provides the implementation**. This preserves dependency direction while allowing each operation to use the model best suited to its purpose.

## Persistence

`BookMyCinema.Persistance` is a separate assembly with one job: persistance and data access concerns.

Persistence owns concrete storage implementation details, including:
- database setup
- ORM setup and configurations (using EF Core as primary ORM)
- entity configurations
- migrations
- seeding
- persistence-only entity models
- interceptors
- data access implementations of repositories and read/write services

### data access implementation

Once an Application or Domain abstraction requires database access, its concrete implementation belongs in the Persistence layer, with the implementation shaped by whether it serves aggregate persistence or a direct read/write use case:

- aggregate repository implementations belong in Persistence, near the aggregate they support
- keep repository implementations focused on aggregate lifecycle and domain persistence
- reader/writer implementations also belong in Persistence near the persistence model they support 
- do not create one repository per table as a default pattern


### Persistence-only models

`Persistance/Country/Country.cs` and `Persistance/Currency/Currency.cs` are clear repository examples of persistence-only models.
These classes are internal and database-shaped.
They are not domain aggregates and are not exposed outside the assembly.

This is the right place for:

- lookup tables
- denormalized read-side entities
- schema-supporting tables
- storage models that exist for persistence or query reasons rather than domain behavior

### Entity configurations

Each persistence model currently has a colocated configuration:

- `Country/Country.cs`
- `Country/CountryConfiguration.cs`
- `Currency/Currency.cs`
- `Currency/CurrencyConfiguration.cs`

That is the current file placement rule to follow:

- put each EF configuration next to the persistence model it configures
- keep table name, key, length, nullability, and related DB mapping decisions in the Persistence project

### Interceptors and audit concerns

`Interceptors/AuditableEntitiesInterceptor.cs` shows an important cross-layer pattern:

- the interceptor lives in Persistence because it is an EF Core concern
- it depends on application-owned abstractions:
  - `ICurrentUserService`
  - `IDateTimeProvider`
- it applies audit interfaces defined in SharedKernel

This is an example of inward dependency preserved through abstractions:

- SharedKernel defines audit capability interfaces
- Application defines application-facing time and current-user contracts
- Persistence uses those contracts while remaining the implementation owner of EF interception


### Migrations and schema artifacts

`Persistance/Migrations` contains EF Core migrations and the model snapshot.
That folder belongs exclusively to database evolution artifacts generated from the Persistence model.

Related schema design documentation lives under `docs/persistence/`, especially [Logical Database Schema](../persistence/logical-schema.md).
That document is the right place for deeper schema rationale, ownership keys, and database invariants.
This architecture document only uses it to explain where persistence concerns belong.


## Infrastructure

`BookMyCinema.Infrastructure` is a separate assembly from Persistence and should stay separate conceptually.
Its role is technical integration outside database storage concerns such as:

- external systems integrations
- Azure or other cloud integrations
- messaging integrations
- storage providers
- email or SMS providers
- external identity adapters
- technical helpers that are not database concerns


## API

`BookMyCinema.Api` is the HTTP boundary (A presentation layer).
It is not the host and not the composition root.
It owns:
- endpoint
- routing concerns
- HTTP request/response separate contracts when needed
- endpoints documentation
- HTTP-specific result mapping
- global exception-to-ProblemDetails handling
- HTTP middlewares specific to the API boundary


### Endpoint organization

Endpoints are organized by API area under `Api/`, then by feature, then by specific endpoint folder.

That gives the repository a clear API side vertical-slice-like structure:

- area routing helpers at the area level
- request/response/documentation/endpoint files grouped together per endpoint

### Endpoint abstraction and discovery

Endpoints implement `IEndpoint`.
`EndpointsRegisterationExtensions` scans the API assembly and registers all non-abstract endpoint classes as `IEndpoint`.

At runtime, `WebApp` resolves `IEnumerable<IEndpoint>` and calls `MapEndpoint` for each endpoint inside the `/api` base group.

This means:

- endpoint mapping stays inside the API assembly
- endpoint discovery is convention-based
- WebApp does not need to know each endpoint individually

### Routes and groupers

The repository currently uses small routing helpers:

- `ApiRoutes` for the global API base.
- feature specfic (API group specific) route constants lives in another file (i.e `TicketsRoutes`) Keeping route strings close to the API feature they belong to.
- `TicketsGrouper` for ticket route grouping

### API contracts vs Application DTOs

- application DTOs or read models owned by use cases can be the same used by APIs, and only when needed they can be separate and mapping will be introduced.
  for example when:
    - the HTTP contract shape differs from the application DTO
    - the API needs transport-specific naming or future versioning
    - the endpoint should hide internal application fields


### Endpoint-to-Application interaction

- endpoint receives HTTP request input
- endpoint maps to application use-case dto (command/query) if they differ.
- endpoint calls Application use-case code
- use case returns `Result` or `Result<T>`
- API maps success to an HTTP response and failures through centralized result mapping

The endpoint should stay thin.

### Result and error handling at the boundary

`Common/Results/ResultsExtensions.cs` centralizes translation from the repository's `Result` model into HTTP responses:

- validation failures become `ValidationProblem`
- other failures become `Problem`
- HTTP status code comes from `ErrorKind`
- machine-readable error codes are included in response extensions

That means Application and Domain should return application owned errors and results, and API should remain the place that decides HTTP formatting.
 See [Error Handling](../cross-cutting/error-handling.md).

`Common/Errors/GlobalExceptionHandler.cs` handles unexpected failures and returns standardized `500` responses.

### Logging helpers

`Common/Logging/` contains HTTP logging-specific concerns:

- logging metadata attribute and options
- request and response body capture middleware
- endpoint extension for adding logging metadata

This is API-layer code because it is HTTP boundary behavior, even though the actual Serilog sink configuration lives in WebApp.


## WebApp

`BookMyCinema.WebApp` is the executable host and composition root.
This separation is one of the clearest architectural decisions in the solution.
This layer is responsbile for assembling and wiring all layers together AND being the execuatable host.
It's responsible for:
    - composition of all layers
    - middleware pipeline assembly
    - host-level logging setup
    - environment and configuration driven startup concerns
    - host startup

### Current implementation

`Program.cs` currently:

- creates the builder
- registers:
  - `AddWeb()`
  - `AddApi()`
  - `AddApplication()`
  - `AddPersistence(...)`
  - `AddInfrastructure()`
- adds Serilog
- builds the app
- configures the web application pipeline
- runs the host

`WebApplicationExtensions.cs` currently handles:

- exception handler middleware
- OpenAPI mapping
- HTTPS redirection
- endpoint mapping
- Serilog request logging and HTTP body capture middleware

`HostBuilderExtensions.cs` owns the Serilog SQL sink configuration, including separate logging tables for normal logs and HTTP logs.

### Why this project exists separately from API

Keeping the host separate prevents the API layer from becoming the place where technical composition leaks inward.
Without `WebApp`, it would be easy to let the API project reference Infrastructure or Persistence just to register services.
The current structure avoids that.
