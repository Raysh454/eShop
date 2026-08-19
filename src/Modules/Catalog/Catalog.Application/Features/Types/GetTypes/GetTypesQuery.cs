using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Types.GetTypes;

public record GetTypesQuery() : IQuery<IReadOnlyList<CatalogTypeDto>>;
