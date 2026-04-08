using BookMyCinema.Domain.Common.Errors;
using FluentValidation.Results;

namespace BookMyCinema.Application.Common.Validations;
internal static class ValidationExtensions
{
    internal static List<Error> ToErrors(this ValidationResult result)
    {
        return result.Errors
            .Select(f => new Error(
                code: f.ErrorCode ?? "Validation.Unknown",
                type: ErrorType.Validation,
                message: f.ErrorMessage,
                field: string.IsNullOrWhiteSpace(f.PropertyName)
                    ? null
                    : ExtractFieldName(f.PropertyName)
            ))
            .ToList();
    }

    //Address.Area -> Area 
    private static string ExtractFieldName(string propertyName)
    {
        var lastDot = propertyName.LastIndexOf('.');
        return lastDot >= 0 ? propertyName[(lastDot + 1)..] : propertyName;
    }

    /*
        example usage:
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result<T>.Failure(validationResult.ToErrors());
        }
    */
}
