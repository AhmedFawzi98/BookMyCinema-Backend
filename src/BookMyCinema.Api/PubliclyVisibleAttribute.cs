namespace BookMyCinema.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum)]
internal sealed class PubliclyVisibleAttribute : Attribute { }
