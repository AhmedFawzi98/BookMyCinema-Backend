namespace BookMyCinema.Domain.Common.Audit;

public interface IModificationAuditable
{
    DateTime? ModifiedAtUtc { get; set; }
    int? ModifiedByUserId { get; set; }
}
