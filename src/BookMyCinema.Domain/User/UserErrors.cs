using BookMyCinema.Domain.Common.Errors;

namespace BookMyCinema.Domain.User;
public static class UserErrors
{
    public static readonly Error EmailTaken =
     new("User.Email.Taken", ErrorType.Conflict, "Email is already in use", "email");

    public static Error NotFound(int id) =>
        new("User.NotFound", ErrorType.NotFound, $"User with id: '{id}' was not found");

    public static readonly Error AccountSuspended =
        new("User.AccountSuspended", ErrorType.Forbidden, "Account is suspended");
}

