# Error Handling

## Results Pattern

BookMyCinema uses a result-based flow for expected failures and exceptions for unexpected failures, keeping business and validation failures explicit while leaving truly exceptional conditions to the host-level exception pipeline.

## Why This Approach

This pattern is a good fit for the codebase because it:

- avoids using exceptions for normal application flow as throwing and handling exceptions is comparatively expensive and disrupts the normal control flow, while result-based failures are explicit, predictable, and more appropriate for conditions the application expects to occur.
- makes business outcomes explicit and keeps code flow easy to follow and self documenting.
- avoids exception-heavy application logic
- produces consistent API errors


## Core Idea

Application services return `Result` or `Result<T>` to explicitly model success and failure, with support for multiple errors primarily for validation failures.

Use `Result` and `Result<T>` when a failure is part of normal domain or application behavior and expected:

- api validation failures
- domain rule violations
- not found, conflict, authorization-related failures
- external dependencies excpetions that are expected

Use exceptions for problems that are not part of the normal use-case flow:

- unexpected infrastructure (outer layers) failures .. only the unexpected ones, as expected ones are to be caught and mapped to results nomrally.
- unexpected runtime faults

That distinction keeps application code readable and avoids using exceptions as control flow.

## Result Types

The shared kernel defines:

- `Result`
- `Result<T>`
- `Error`
- `ErrorKind`

`Result<T>` carries a value on success and a list of errors on failure. The non-generic `Result` is used when a use case succeeds without returning data.

Multiple errors are supported so one failed result can represent more than one problem. This is mainly used for validation failures, where a single request can have several invalid fields.

The types also include implicit conversions (implecit operators) so code can stay concise.

```csharp
return ticket;
return TicketErrors.NotFound;
```

## Error Shape

Each error carries the fields needed for both humans and clients:

- `Code`
- `Type`
- `Message`
- optional `Field` for API errors only.

This lets the API layer return errors in a consistent format while still preserving enough detail for debugging and UI mapping.

## Error Catalogs

Business errors are organized in domain-level catalogs. These catalogs act as both reusable error definitions and documentation of domain rules.
They describe business failures such as violated invariants, missing domain objects, conflicts, or other expected domain outcomes.

API request validation errors are kept separate from domain error catalogs.
Request validation is about the shape and correctness of an API request before the use case can run. It is not domain validation, so those errors are defined in a separate catalog per use case, close to the validator and request model they belong to.

This avoids reusing domain error catalogs for request-shape problems, because domain catalogs are focused on business rules rather than API request correctness. although sometimes there might be duplicate validations but they are enforcing rules across separate layers.

## Validation Flow

Request validation uses FluentValidation in the application layer. Validation failures are converted into structured errors through `ValidationExtensions.ToErrors()`.

That conversion normalizes the validation output so the rest of the pipeline only deals with the project's own error model.

## HTTP Mapping

The presentation layer translates `Result` values into RFC-compliant HTTP responses through a centralized mapping at the API boundary.

The mapping is intentionally centralized:

- validation errors become `ValidationProblemDetails`
- other error kinds become `ProblemDetails`
- status codes are derived from `ErrorKind`
- validation errors are grouped consistently by field
- machine-readable error codes are included in the response extensions

A `Match` abstraction is used at the API boundary so endpoints keep full control over successful responses, such as `200 OK`, `201 Created`, or `204 No Content`, while failure handling remains centralized and consistent.

That keeps endpoint code focused on the happy path and avoids repeating response mapping in every handler.

## Global Exception Handling

Unexpected failures are handled by a global exception handler registered at the host level.
It catches unhandled exceptions and converts them into standardized, non-leaking `500` responses using `ProblemDetails`.
This gives the API a stable failure shape even when something escapes the result-based flow.

Infrastructure concerns, such as database failures or external service failures, rely on exceptions rather than `Result` values.
This preserves the distinction between expected application outcomes and exceptional infrastructure faults, and keeps infrastructure concerns out of normal domain and application flow.

Application logic may still translate specific infrastructure exceptions into meaningful domain errors when those failures are expected and useful to expose as part of the use case.
