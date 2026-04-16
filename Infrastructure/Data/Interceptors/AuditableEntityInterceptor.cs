using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Common.Interfaces;

namespace Infrastructure.Data.Interceptors
{
    internal sealed class AuditableEntityInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
    {
        void UpdateDatetimeAudit(DbContext? context)
        {
            if (context is null) return;

            var entries = context.ChangeTracker.Entries<IDatetimeAudit>();

            foreach (var entry in entries)
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    if (entry.State is EntityState.Added)
                    {
                        entry.Entity.CreatedAt = timeProvider.GetUtcNow();
                    }
                    entry.Entity.ModifiedAt = timeProvider.GetUtcNow();
                }
            }
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateDatetimeAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateDatetimeAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
