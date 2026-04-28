using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Data.Interceptors
{
    internal sealed class DispatchDomainEventInterceptor(IMediator mediator) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task DispatchDomainEventsAsync(DbContext? context, CancellationToken cancellationToken = default)
        {
            if (context is null)
            {
                return;
            }

            var entries = context.ChangeTracker.Entries<IDomainEvent>();

            var entities = entries
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToArray();

            var domainEvents = entities
                .SelectMany(entity => entity.DomainEvents)
                .ToArray();

            foreach (var entity in entities)
            {
                entity.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
