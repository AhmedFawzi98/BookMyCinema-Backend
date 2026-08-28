namespace BookMyCinema.Application.Common.Abstractions.Persistence;

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);

    Task RollbackAsync(CancellationToken ct = default);

    Task CreateSavepointAsync(string savepointName, CancellationToken ct = default);

    Task RollbackToSavepointAsync(string savepointName, CancellationToken ct = default);
}
