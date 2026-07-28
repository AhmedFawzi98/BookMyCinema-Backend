namespace BookMyCinema.Application.User;

public interface ICurrentUserService
{
    int? UserId { get; }
    // its implmenation might be in application layer, or maybe in api layer to access http context without need to use IHttpContextAccessor in application layer
    //to be decided..
}
