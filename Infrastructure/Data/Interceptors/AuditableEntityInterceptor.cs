using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Common.Interfaces;

namespace Infrastructure.Data.Interceptors
{
    internal sealed class AuditableEntityInterceptor(TimeProvider timeProvider, IUser currentUser) : SaveChangesInterceptor
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

        void UpdateUserAudit(DbContext? context)
        {
            if (context is null) return;

            var entries = context.ChangeTracker.Entries<IUserAudit>();

            foreach (var entry in entries)
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    if (entry.State is EntityState.Added)
                    {
                        entry.Entity.CreatedBy = currentUser.Id;
                    }
                    entry.Entity.ModifiedBy = currentUser.Id;
                }
            }
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateDatetimeAudit(eventData.Context);
            UpdateUserAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateDatetimeAudit(eventData.Context);
            UpdateUserAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
