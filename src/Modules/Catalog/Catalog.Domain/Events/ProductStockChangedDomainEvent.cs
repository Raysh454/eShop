using BuildingBlocks.Domain;

namespace Catalog.Domain.Events;

public sealed record ProductStockChangedDomainEvent(CatalogItem Item, int PreviousStock, int NewStock) : DomainEvent;
