namespace BookMyCinema.Domain.Common.Audit;

public interface IActivationAuditable
{
    //implmneation will add private setter and default value, as activiation state is based on domain logic and controlled within the entitiy itself
    bool IsActive { get; }
    DateTime? ActivationChangedAtUtc { get; }
    int? ActivationChangedByUserId { get; }
}
