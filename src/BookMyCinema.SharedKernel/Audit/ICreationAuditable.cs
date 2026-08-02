namespace BookMyCinema.Domain.Common.Audit;

public interface ICreationAuditable
{
    DateTime CreatedAtUtc { get; set; }
    int? CreatedByUserId { get; set; }
}
