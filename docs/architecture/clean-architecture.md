# BookMyCinema Architecture

BookMyCinema follows a domain-centric Architecture. Applying Clean Architecture.
The domain and application layers form the core of the system, while outer layers provide delivery, persistence, infrastructure, framework dependent and other technical capabilities around that core.

The current codebase is organized around a set of assemblies:

- `BookMyCinema.Domain`
- `BookMyCinema.SharedKernel`
- `BookMyCinema.Application`
- `BookMyCinema.Api`
- `BookMyCinema.Persistance`
- `BookMyCinema.Infrastructure`
- `BookMyCinema.WebApp`

The direction of dependencies is intentionally one way: outer layers depend on inner layers, while the core inner layers, such as Domain and Application, do not depend on outer-layer concerns related to persistence, external dependencies, frameworks, or specific technologies.


## Why Clean Architecture

The primary goal of this architecture is **clarity through explicit boundaries and responsibilities**:

- **Business rules remain isolated from frameworks and infrastructure**, keeping the core domain focused on business behavior rather than technical concerns.
- **Infrastructure and persistence are implementation details**, accessed through abstractions where appropriate so they can evolve without affecting core business logic.
- **Use cases are organized by feature and kept self-contained where practical**, making related behavior easy to locate, understand, modify, and test.
- **Dependencies flow in a deliberate direction**, preventing infrastructure and presentation concerns from leaking into the application and domain layers.
- **Expected failures are represented explicitly through results**, while exceptions are reserved for exceptional or unexpected failures. See [Results Pattern](../error-handling/results-pattern.md).
- **Architectural boundaries are enforceable and testable**, allowing automated tests to detect invalid dependencies and protect the intended structure as the codebase evolves. See [Architecture Testing](../testing/architecture-testing.md).

The objective is not to maximize abstraction or layering, but to make the system's **business logic, dependencies, use cases, and failure paths explicit and predictable**.


## Layer Responsibilities

### Domain

The Domain contains the core business model and rules, expressed using DDD concepts:

- Aggregates and aggregate roots
- Entities and value objects
- Domain services
- Domain events
- Business rules and invariants
- Domain-specific errors


This layer stays free of framework dependencies so business rules remain portable and easy to reason about.


### Shared Kernel

`BookMyCinema.SharedKernel` holds domain primitives / bounded contexts shared building blocks:

- base entity and aggregate root
- audit interfaces
- shared value objects
- domain events abstractions
- results and error types

This avoids duplicating cross-cutting domain concepts across mutliple bounded contexts.

Results are placed in shared kernel just to group by use, otherwise it could be placed in another non-domain focused shared layer such as Common layer.


### Application

The application layer coordinates use cases. It contains feature folders such as `Features/Tickets`, thats sub-divided into use cases such as:

- commands and queries live close to the use case they serve
- FluentValidation is used for request validation, lives in it's use case folder.
- each use case keeps its validation errors separate from domain error catalogs and close to the validation rules they belong to.
- api validation failures are converted into structured domain errors before they leave the use case boundary
- application services depend on abstractions when implmentations lives in another layer.

### API

The API layer is the application's HTTP entry point, responsible for exposing the application's capabilities to clients:

- Endpoint definition, routing, and API documentation
- Request and response contracts (when needed, they are separate from application DTOs such as: commands/queries/results DTOs.
- HTTP-specific result and error mapping
- Global exception handling
- HTTP request/response logging control 

Endpoints implement `IEndpoint`, and the web application discovers them through DI and maps them into a shared route group. 


### Persistence

The persistence layer owns concrete implmentation of persistence concerns, database setup, ORM configurations, entity configurations, interceptors and implmentation of data access.
so it is responsible for:

- database configuration and setup
- connection string and EF options binding
- entity configuration via `ApplyConfigurationsFromAssembly` and per entity `IEntityTypeConfiguration`
- automatic audit field population
- concrete implmentation of data access of domain repositories interfaces and application command(writers) and query(readers) services

### Infrastructure

- todo:

### WebApp

`BookMyCinema.WebApp` is the host/composition root. It wires the application together:

- configures hosting concerns.
- configure logging
- registers services from all layers
- register middlewares and map endpoints.
- enables OpenAPI and HTTPS redirection

A dedicated host project is used intentionally to keep composition concerns separate from the API layer. This prevents dependency-rule workarounds, such as making the API reference Infrastructure solely to register its services.
It also provides a single place that is allowed to reference and compose all application layers while keeping their architectural dependencies intact.
