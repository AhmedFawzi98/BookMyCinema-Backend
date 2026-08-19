using BookMyCinema.Application.Common.Abstractions;
using BookMyCinema.Application.User;
using BookMyCinema.Domain.Common.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookMyCinema.Persistance.Interceptors;

internal class AuditableEntitiesInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if(eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;
        int? userId = currentUserService.UserId;

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is ICreationAuditable creationAuditable)
            {
                creationAuditable.CreatedAtUtc = utcNow;
                creationAuditable.CreatedByUserId = userId;
            }

            if (entry.State == EntityState.Modified && entry.Entity is IModificationAuditable modificationAuditable)
            {
                modificationAuditable.ModifiedAtUtc = utcNow;
                modificationAuditable.ModifiedByUserId = userId;
            }
        }
    }
}
