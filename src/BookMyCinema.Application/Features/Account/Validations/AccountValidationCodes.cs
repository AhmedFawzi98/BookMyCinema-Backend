namespace BookMyCinema.Application.Features.Account.Validations;
internal static class AccountValidationCodes
{
    internal static class Register
    {
        public const string EmailRequired = "Validation.Account.Register.Email.Required";
        public const string PasswordRequired = "Validation.Account.Register.Password.Required";
        public const string ConfirmPasswordMismatch = "Validation.Account.Register.ConfirmPassword.Mismatch";
    }

    internal static class Login
    {
        public const string EmailRequired = "Validation.Account.Login.Email.Required";
        public const string PasswordRequired = "Validation.Account.Login.Password.Required";
    }

    internal static class ResetPassword
    {
        public const string EmailRequired = "Validation.Account.ResetPassword.Email.Required";
    }
}
