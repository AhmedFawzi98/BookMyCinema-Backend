namespace BookMyCinema.Application;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum)]
internal sealed class PubliclyVisibleAttribute : Attribute { }
