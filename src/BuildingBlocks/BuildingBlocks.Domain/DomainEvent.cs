namespace BuildingBlocks.Domain;

// <summary> Base record carrying the identity and timestamp every domain event
// needs, so concrete events only declare their payload. </summary>

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
