# Validation

BookMyCinema uses FluentValidation.

Current behavior and placement rules:

- validators live in Application, not API
- request-shape validation stays near the use case it belongs to
- validation error codes/messages (catalogues) are kept in a dedicated files next to the validator they are used in (per use case) and separate from domain error catalogues
- validation failures are converted into application owned `Error` instances through `Common/Validations/ValidationExtensions.cs`

## Flow

The normal flow is:

- API receives HTTP input
- API maps that input to an application command or query object (if they are separate)
- Application runs FluentValidation against the application model (via use case handler validation decorator by default, or manually injected and invoked in use case handler if needed)
- validation failures are mapped into application-owned `Error` objects
- the use case returns `Result` or `Result<T>` with those errors


## FluentValidation registration and placement

Validators are registered from the application assembly in `src/BookMyCinema.Application/ServiceCollectionExtensions.cs` using `AddValidatorsFromAssemblyContaining(..., includeInternalTypes: true)`.

That keeps validators:

- close to the use case
- internal to Application by default
- automatically discoverable by DI

## Mapping validation failures to application-owned errors

FluentValidation is only the validation engine. The application still owns the error model exposed to the rest of the solution.

`src/BookMyCinema.Application/Common/Validations/ValidationExtensions.cs` maps:

- a single `ValidationResult` to `List<Error>`
- multiple `ValidationResult` instances to `List<Error>`

The mapping keeps:

- `ErrorKind.Validation` as the error type
- `ValidationFailure.ErrorCode` as the application error code when provided
- `ValidationFailure.ErrorMessage` as the message
- a normalized field name extracted from the FluentValidation property path

This is the bridge between FluentValidation and the rest of application result/error model.

## Validation decorators for command handlers

The repository uses validation decorator so command handlers do not need to inject validators manually. (i.e `ValidationDecorator`)

The decorator wrap:

- `ICommandHandler<TCommand>`
- `ICommandHandler<TCommand, TResponse>`

Each decorator receives `IEnumerable<IValidator<TCommand>>` from DI, runs all validators for that command, gathers the validation results, converts invalid results through `ValidationExtensions`, and returns a failed `Result` or `Result<TResponse>` before the actual handler executes.

This gives one centralized validation path for command handlers.

See [CQRS](../architecture/cqrs.md), section `Decorators as pipeline behavior`.

