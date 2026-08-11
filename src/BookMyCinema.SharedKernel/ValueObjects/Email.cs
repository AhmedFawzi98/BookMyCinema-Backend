using System.Text.RegularExpressions;
using BookMyCinema.Domain.Common.Errors;
using BookMyCinema.Domain.Common.Results;

namespace BookMyCinema.SharedKernel.ValueObjects;

public sealed partial record Email
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string? value)
    {
        value = value?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return EmailErrors.Required;
        }

        value = value.ToLowerInvariant();
        if (!IsValidEmail(value))
        {
            return EmailErrors.InvalidFormat;
        }

        return new Email(value);
    }

    private static bool IsValidEmail(string value)
    {
        // Actual implementation can be stronger.
        return EmailRegex().IsMatch(value);
    }
}

public sealed partial record Email
{
    [GeneratedRegex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}

public static class EmailErrors
{
    public static readonly Error Required =
        new(
            "Email.Required",
            ErrorKind.RuleViolation,
            "Email is required.");

    public static readonly Error InvalidFormat =
        new(
            "Email.InvalidFormat",
            ErrorKind.RuleViolation,
            "Email format is invalid.");
}
