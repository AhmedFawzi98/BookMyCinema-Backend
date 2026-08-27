namespace BookMyCinema.Application.Common.Abstractions.Messaging;

public interface ICommandBase
{
}

public interface ICommand : ICommandBase
{
}

public interface ICommand<TResponse> : ICommandBase
{
}


