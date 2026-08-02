using BookMyCinema.Domain.Common.Errors;

namespace BookMyCinema.Domain.User;
public static class UserErrors
{
    public static readonly Error EmailTaken =
     new("User.Email.Taken", ErrorKind.Conflict, "Email is already in use", "email");

    public static Error NotFound =>
        new("User.NotFound", ErrorKind.NotFound, "User with id: {Id} was not found");

    public static readonly Error AccountNotActive =
        new("User.Account.NotActive", ErrorKind.AccessDenied, "Account is not active.");

    public static readonly Error InvalidCredentials =
        new("User.Account.InvalidCredentials", ErrorKind.AuthenticationFailure, "The email or password is incorrect.");
}

