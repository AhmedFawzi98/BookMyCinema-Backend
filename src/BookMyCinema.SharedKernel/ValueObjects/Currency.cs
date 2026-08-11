using BookMyCinema.Domain.Common.Errors;
using BookMyCinema.Domain.Common.Results;

public sealed record Currency
{
    private const int ExpectedLength = 3;

    private Currency(string code)
    {
        Code = code;
    }

    public string Code { get; }

    public static Result<Currency> Create(string? code)
    {
        code = code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            return CurrencyErrors.Required;
        }

        code = code.ToUpperInvariant();
        if (code.Length != ExpectedLength || !code.All(char.IsLetter))
        {
            return CurrencyErrors.InvalidFormat;
        }

        return new Currency(code);
    }
}

public static class CurrencyErrors
{
    public static readonly Error Required =
        new(
            "Currency.Required",
            ErrorKind.RuleViolation,
            "Currency is required.");

    public static readonly Error InvalidFormat =
        new(
            "Currency.InvalidFormat",
            ErrorKind.RuleViolation,
            "Currency format is invalid.");
}
