using BookMyCinema.Application.Common.Abstractions.Messaging;
using BookMyCinema.Domain.Common.Results;
using FluentValidation;
using FluentValidation.Results;

namespace BookMyCinema.Application.Common.Validations;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            IReadOnlyList<ValidationResult> validationResults = await ValidateAsync(command, validators, cancellationToken);

            IReadOnlyList<ValidationResult> nonValidResults = validationResults
                .Where(validationResult => !validationResult.IsValid)
                .ToList()
                .AsReadOnly();

            if (nonValidResults.Count == 0)
            {
                return await innerHandler.HandleAsync(command, cancellationToken);
            }

            return Result<TResponse>.Failure(nonValidResults.ToErrors());
        }
    }

    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            IReadOnlyList<ValidationResult> validationResults = await ValidateAsync(command, validators, cancellationToken);

            IReadOnlyList<ValidationResult> nonValidResults = validationResults
                .Where(validationResult => !validationResult.IsValid)
                .ToList()
                .AsReadOnly();

            if (nonValidResults.Count == 0)
            {
                return await innerHandler.HandleAsync(command, cancellationToken);
            }

            return Result.Failure(nonValidResults.ToErrors());
        }
    }

    private static async Task<IReadOnlyList<ValidationResult>> ValidateAsync<TCommand>(
        TCommand command,
        IEnumerable<IValidator<TCommand>> validators,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TCommand>(command);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        return validationResults.ToList().AsReadOnly();
    }
}
