using BuildingBlocks.Domain;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Events;

public sealed record ProductPriceChangedDomainEvent(CatalogItem Item, Money OldPrice, Money NewPrice) : DomainEvent;
