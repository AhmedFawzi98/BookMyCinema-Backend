namespace BookMyCinema.Domain.Common.Errors;

public class Error
{
    public string Code { get; } = string.Empty;

    public ErrorKind Type { get; }

    public string Message { get; } = string.Empty;

    public string? Field { get; } = string.Empty;

    public Error(string code, ErrorKind type, string message, string? field = null)
    {
        Code = code;
        Type = type;
        Message = message;
        Field = field;
    }
}

