using BookMyCinema.Domain.Common.Errors;
using BookMyCinema.Domain.Common.Results;
using Microsoft.AspNetCore.Http;
using IResult = Microsoft.AspNetCore.Http.IResult;
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
        bool isValidationError = errors.All(e => e.Type == ErrorKind.Validation);

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
                title: MapTitle(ErrorKind.Validation),
                statusCode: MapStatusCode(ErrorKind.Validation),
                errors: errorDict,
                extensions: new Dictionary<string, object?>
                {
                    ["codes"] = codeDict
                });
        }

        Error first = errors[0];

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

    private static int MapStatusCode(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation =>
            StatusCodes.Status400BadRequest,

        ErrorKind.AuthenticationFailure =>
            StatusCodes.Status401Unauthorized,

        ErrorKind.AccessDenied =>
            StatusCodes.Status403Forbidden,

        ErrorKind.NotFound =>
            StatusCodes.Status404NotFound,

        ErrorKind.Conflict =>
            StatusCodes.Status409Conflict,

        ErrorKind.RuleViolation =>
            StatusCodes.Status422UnprocessableEntity,

        _ =>
            StatusCodes.Status500InternalServerError
    };

    private static string MapTitle(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation =>
            "Validation error",

        ErrorKind.AuthenticationFailure =>
            "Authentication failed",

        ErrorKind.AccessDenied =>
            "Access denied",

        ErrorKind.NotFound =>
            "Resource not found",

        ErrorKind.Conflict =>
            "Conflict",

        ErrorKind.RuleViolation =>
            "Business rule violation",

        _ =>
            "Server error"
    };
}
