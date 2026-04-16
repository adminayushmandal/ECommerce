namespace Domain.Common.Interfaces
{
    public interface IDomainEvent
    {
        IReadOnlyCollection<BaseEvent> DomainEvents { get; }
        void AddDomainEvent(BaseEvent domainEvent);
        void ClearDomainEvents();
    }
}
