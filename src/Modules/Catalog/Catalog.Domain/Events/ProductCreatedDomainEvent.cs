using BuildingBlocks.Domain;

namespace Catalog.Domain.Events;

public sealed record ProductCreatedDomainEvent(string Name) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
