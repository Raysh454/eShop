using BuildingBlocks.Domain;

namespace Catalog.Domain.Events;

// <summary> Carries the aggregate rather than a snapshot: the identity is not
// assigned until the item is tracked, and events are dispatched during
// SaveChanges, by which point Id is populated. </summary>

public sealed record ProductCreatedDomainEvent(CatalogItem Item) : DomainEvent;
