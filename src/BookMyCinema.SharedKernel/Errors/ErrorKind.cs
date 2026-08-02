namespace BookMyCinema.Domain.Common.Errors;

public enum ErrorKind
{
    Validation,
    AuthenticationFailure,
    AccessDenied, 
    NotFound,
    Conflict,
    RuleViolation, 
}
