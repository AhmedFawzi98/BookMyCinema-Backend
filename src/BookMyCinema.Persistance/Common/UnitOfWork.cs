using System.Data;
using BookMyCinema.Application.Common.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookMyCinema.Persistance.Common;

internal sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        //TODO: will handle different exceptions later (uniqeness, Concurrency excpeitons with extension methods, etc..) either by mapping to resulsts or throwing application owned custom exception and handle in use case handler
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(isolationLevel, ct);
        return new DbContextTransaction(tx);
    }
}
