using BookMyCinema.Domain.Common.Errors;
using FluentValidation.Results;

namespace BookMyCinema.Application.Common.Validations;

internal static class ValidationExtensions
{
    internal static List<Error> ToErrors(
      this IReadOnlyList<ValidationResult> results)
    {
        return results
            .SelectMany(result => result.Errors)
            .Select(ToError)
            .ToList();
    }

    internal static List<Error> ToErrors(this ValidationResult result)
    {
        return result.Errors
            .Select(ToError)
            .ToList();
    }
    private static Error ToError(ValidationFailure validationFailure)
    {
        return new Error(
            code: validationFailure.ErrorCode ?? "Validation.Unknown",
            type: ErrorKind.Validation,
            message: validationFailure.ErrorMessage,
            field: string.IsNullOrWhiteSpace(validationFailure.PropertyName)
                ? null
                : ExtractFieldName(validationFailure.PropertyName));
    }

    //Address.Area -> Area 
    private static string ExtractFieldName(string propertyName)
    {
        int lastDot = propertyName.LastIndexOf('.');
        return lastDot >= 0 ? propertyName[(lastDot + 1)..] : propertyName;
    }
}
