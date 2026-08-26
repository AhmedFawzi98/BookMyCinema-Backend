# Validation

The repository uses FluentValidation in Application, registered by `AddValidatorsFromAssembly` in `Application/ServiceCollectionExtensions.cs`.

Current behavior and placement rules:

- validators live in Application, not API
- request-shape validation stays near the use case it belongs to
- validation error codes/messages are kept in a dedicated local file next to the validator
- validation failures are converted into `Error` instances through `Common/Validations/ValidationExtensions.cs`

