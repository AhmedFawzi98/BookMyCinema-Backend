using System;
using System.Collections.Generic;
using System.Text;
using BookMyCinema.Application.Common.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookMyCinema.Persistance.Common;

internal sealed class DbContextTransaction(IDbContextTransaction dbContextTransaction) : ITransaction
{
    public Task CommitAsync(CancellationToken ct = default)
        => dbContextTransaction.CommitAsync(ct);
   
    public Task CreateSavepointAsync(string savepointName, CancellationToken ct = default)
        => dbContextTransaction.CreateSavepointAsync(savepointName, ct);

    public Task RollbackAsync(CancellationToken ct = default)
        => dbContextTransaction.RollbackAsync(ct);

    public Task RollbackToSavepointAsync(string savepointName, CancellationToken ct = default)
        => dbContextTransaction.RollbackToSavepointAsync(savepointName, ct);

    public ValueTask DisposeAsync()
        => dbContextTransaction.DisposeAsync();
}
