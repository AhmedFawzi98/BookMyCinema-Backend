using BookMyCinema.Application.Common.Abstractions;
using BookMyCinema.Domain.Common.Errors;

namespace BookMyCinema.Application.Common.Results;

public class Result<T> : IResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public IReadOnlyList<Error> Errors { get; }

    private Result(bool isSuccess, T? value, List<Error> errors)
    {
        if (isSuccess && errors.Count > 0)
        {
            throw new ArgumentException("Success must have no errors");
        }
        if (!isSuccess && (errors is null || errors.Count == 0))
        {
            throw new ArgumentException("Failure must have errors");
        }

        IsSuccess = isSuccess;
        Value = value;
        Errors = errors.AsReadOnly();
    }

    public static Result<T> Success(T value) => new(true, value, []);

    public static Result<T> Failure(List<Error> errors) => new(false, default(T), errors: errors);

    public static Result<T> Failure(params Error[] errors) =>
      new(false, default(T), errors.ToList());

    //return UserErrors.EmailTaken; instead of return Result<SomeType>.Failure(UserErrors.EmailTaken);
    public static implicit operator Result<T>(T value) => Success(value);

    //return someType; instead of return Result<SomeType>.Success(someType);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

public class Result : IResult
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<Error> Errors { get; }

    private Result(bool isSuccess, List<Error> errors)
    {
        if (isSuccess && errors.Count > 0)
        {
            throw new ArgumentException("Success must have no errors");
        }
        if (!isSuccess && (errors is null || errors.Count == 0))
        {
            throw new ArgumentException("Failure must have errors");
        }

        IsSuccess = isSuccess;
        Errors = errors.AsReadOnly();
    }

    public static Result Success() => new(true, []);

    public static Result Failure(List<Error> errors) => new(false, errors);

    public static Result Failure(params Error[] errors) =>
      new(false, errors.ToList());

    //return UserErrors.EmailTaken; instead of return Result.Failure(UserErrors.EmailTaken);
    public static implicit operator Result(Error error) => Failure(error);

    //no need for operator overloading for non generic success, use Result.Success()
}
