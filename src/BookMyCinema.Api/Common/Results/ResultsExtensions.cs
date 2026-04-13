using BookMyCinema.Application.Common.Results;
using BookMyCinema.Domain.Common.Errors;
using Microsoft.AspNetCore.Http;
namespace BookMyCinema.Api.Common.Results;
internal static class ResultExtensions
{
    public static IResult Match<T>(
        this Result<T> result,
        Func<T, IResult> onSuccess)
    {
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : MapFailure(result.Errors);
    }

    public static IResult Match(
        this Result result,
        Func<IResult> onSuccess)
    {
        return result.IsSuccess
            ? onSuccess()
            : MapFailure(result.Errors);
    }

    private static IResult MapFailure(IReadOnlyList<Error> errors)
    {
        var isValidationError = errors.All(e => e.Type == ErrorType.Validation);

        if (isValidationError)
        {
            var errorDict = errors
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Field) ? "general" : e.Field!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Message).ToArray()
                );

            var codeDict = errors
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Field) ? "general" : e.Field!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Code).ToArray()
                );

            return Microsoft.AspNetCore.Http.Results.ValidationProblem(
                title: MapTitle(ErrorType.Validation),
                statusCode: MapStatusCode(ErrorType.Validation),
                errors: errorDict,
                extensions: new Dictionary<string, object?>
                {
                    ["codes"] = codeDict
                });
        }

        var first = errors[0];

        //sets both ProblemDetails Status field, and http response status code
        return Microsoft.AspNetCore.Http.Results.Problem(
            title: MapTitle(first.Type),
            statusCode: MapStatusCode(first.Type),
            detail: first.Message,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = first.Code
            });
    }

    private static int MapStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unprocessable => StatusCodes.Status422UnprocessableEntity,
        ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string MapTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation error",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Resource not found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unprocessable => "Unprocessable entity",
        ErrorType.TooManyRequests => "Too many requests",
        _ => "Server error"
    };
}
