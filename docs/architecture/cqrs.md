# CQRS

BookMyCinema uses logical CQRS inside the same application and database, not separate physical read and write systems.

The goal is to model reads and writes according to their different responsibilities:

- commands represent state-changing use cases
- queries represent read-only use cases
- each use case owns its request and result shapes
- handlers execute the use case behind application-owned abstractions

This enables each side to use the most appropriate data-access strategy such as repositories/writers for commands, optimized readers (might even use different ORM) for queries, and purpose-built projected read models without forcing reads through the domain model.
Organizing commands and queries by use case also avoids god services and preserves Screaming Architecture, as BookMyCinema structure reflects what the system does rather than its technical components.

## Message abstractions

Application messaging lives under `src/BookMyCinema.Application/Common/Abstractions/Messaging/`.

Current abstractions:

- `ICommand` for commands that do not return a value, such as when a use case changes state and only needs success/failure
- `ICommand<TResponse>` for commands that return a value, such as when a write use case must also return data, for example a created identifier, summary, or response model.
- `IQuery<TResponse>` for queries
- `ICommandHandler<TCommand>` for commands with no response
- `ICommandHandler<TCommand, TResponse>` for commands with a response
- `IQueryHandler<TQuery, TResponse>` for queries

All handlers return the repository result model.
This keeps success/failure handling explicit and consistent across commands and queries.

## Feature-level flow

BookMyCinema does not implement MediatR-style mediator dispatching. There is no central Send(...) abstraction that receives a request and resolves the corresponding handler at runtime.

Instead, each Minimal API endpoint explicitly injects the command/query handler interface it needs, maps the HTTP request to the application command/query when separate API contracts are used, and calls HandleAsync directly.

This fits naturally with the endpoint-per-file Minimal API structure while keeping dependencies explicit and avoiding runtime handler resolution, reflection-based dispatch, and service-locator-style lookup.

## Decorators as pipeline behavior

BookMyCinema uses decorators as a centralized pipeline behaviour around handlers.

This avoids repeating cross-cutting concerns inside every use case handler, such as:

- validation
- logging


The practical effect is:

- handlers stay focused on use-case behavior
- validators do not need to be injected manually into every command handler
- basic essential logging does not need to be duplicated in each handler
- multiple decorators can be composed around the same handler

## Validation decorators

Validation decorators currently wrap command handlers only:

- `ValidationDecorator.CommandHandler<TCommand>`
- `ValidationDecorator.CommandHandler<TCommand, TResponse>`

Each decorator receives:

- the inner handler
- `IEnumerable<IValidator<TCommand>>`

It runs all validators for the command, gathers all `ValidationResult` instances, filters the invalid ones, maps them to application-owned `Error` objects through `ValidationExtensions`, and returns a failed `Result` or `Result<TResponse>` without entering the actual handler when validation fails.

That means a command handler can stay focused on business flow and does not need this repeated shape:

- inject and calling validator
- convert FluentValidation failures
- return validation failure result manually

## Logging decorators

Logging decorators currently wrap:

- `IQueryHandler<TQuery, TResponse>`
- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`

They log:

- processing start
- successful completion
- failed completion with errors

This creates one centralized logging style for application message handling rather than many per-handler variations.

## DI registration and Scrutor

Handler scanning and decorator registration are implemented in:

- `src/BookMyCinema.Application/ServiceCollectionExtensions.cs`

The current DI setup does two things:

1. Registers concrete handlers by scanning the application assembly.
2. Decorates the handler interfaces using Scrutor `TryDecorate`.

Handler registration covers:

- `IQueryHandler<,>`
- `ICommandHandler<>`
- `ICommandHandler<,>`

Decorator registration currently uses Scrutor in this order:

- validation decorators for `ICommandHandler<>` and `ICommandHandler<,>`
- logging decorators for `IQueryHandler<,>`, `ICommandHandler<,>`, and `ICommandHandler<>`

Scrutor applies the last registered decorator as the outermost one.
So with the current setup, command execution becomes:

- logging decorator
- validation decorator
- actual handler

And query execution becomes:

- logging decorator
- actual handler

## endpoint injection still receives the decorated handler

Endpoints inject handler interfaces, and receive decorated handlers.

DI resolves the final service registered for that interface. Because Scrutor decorates the interface registration, the resolved instance is the decorated chain, not the raw injected handler.

This way:

- API stays unaware of logging and validation plumbing
- handlers stay unaware of decorator composition
- cross-cutting behavior stays centralized in one place
