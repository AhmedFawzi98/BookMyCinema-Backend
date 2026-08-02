using BookMyCinema.Application.Common.Abstractions;

namespace BookMyCinema.Infrastructure;

internal class DatetimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
