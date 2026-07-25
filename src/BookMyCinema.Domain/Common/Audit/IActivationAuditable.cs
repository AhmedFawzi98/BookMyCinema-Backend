namespace BookMyCinema.Domain.Common.Audit;

public interface IActivationAuditable
{
    bool IsActive { get; set; }
    DateTime? ActivationChangedAtUtc { get; set; }
    int? ActivationChangedByUserId { get; set; }
}
