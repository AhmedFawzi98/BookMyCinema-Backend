using BookMyCinema.Domain.Common.Errors;
using BookMyCinema.Domain.Common.Results;

namespace BookMyCinema.SharedKernel.ValueObjects;

public sealed record Country
{
    private const int ExpectedLength = 2;

    private Country(string code)
    {
        Code = code;
    }

    public string Code { get; }

    public static Result<Country> Create(string? code)
    {
        code = code?.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            return CountryErrors.Required;
        }

        code = code.ToUpperInvariant();

        if (code.Length != ExpectedLength || !code.All(char.IsLetter))
        {
            return CountryErrors.InvalidFormat;
        }

        return new Country(code);
    }
}

public static class CountryErrors
{
    public static readonly Error Required =
        new(
            "Country.Required",
            ErrorKind.RuleViolation,
            "Country is required.");

    public static readonly Error InvalidFormat =
        new(
            "Country.InvalidFormat",
            ErrorKind.RuleViolation,
            "Country format is invalid.");
}
