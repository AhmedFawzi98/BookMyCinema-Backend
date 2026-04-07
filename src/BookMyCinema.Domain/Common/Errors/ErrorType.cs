namespace BookMyCinema.Domain.Common.Errors;

public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    Unprocessable,
    TooManyRequests,
    InternalServerError,
}
