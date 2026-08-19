using BuildingBlocks.Application.CQRS;
using Catalog.Application.Abstractions;

namespace Catalog.Application.Features.Types.GetTypes;

public sealed class GetTypesHandler(ICatalogQueries queries)
    : IQueryHandler<GetTypesQuery, IReadOnlyList<CatalogTypeDto>>
{
    public Task<IReadOnlyList<CatalogTypeDto>> Handle(GetTypesQuery request, CancellationToken cancellationToken) =>
        queries.GetTypesAsync(cancellationToken);
}
