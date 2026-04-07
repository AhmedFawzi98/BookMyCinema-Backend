namespace BookMyCinema.Application.Features.Account.Validations;
internal static class AccountValidationCodes
{
    internal static class Register
    {
        internal const string EmailRequired = "Validation.Account.Register.Email.Required";
        internal const string PasswordRequired = "Validation.Account.Register.Password.Required";
        internal const string ConfirmPasswordMismatch = "Validation.Account.Register.ConfirmPassword.Mismatch";
    }

    internal static class Login
    {
        internal const string EmailRequired = "Validation.Account.Login.Email.Required";
        internal const string PasswordRequired = "Validation.Account.Login.Password.Required";
    }

    internal static class ResetPassword
    {
        internal const string EmailRequired = "Validation.Account.ResetPassword.Email.Required";
    }
}
