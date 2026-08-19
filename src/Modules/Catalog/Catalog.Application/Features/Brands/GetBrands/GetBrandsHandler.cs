using BuildingBlocks.Application.CQRS;
using Catalog.Application.Abstractions;

namespace Catalog.Application.Features.Brands.GetBrands;

public sealed class GetBrandsHandler(ICatalogQueries queries)
    : IQueryHandler<GetBrandsQuery, IReadOnlyList<CatalogBrandDto>>
{
    public Task<IReadOnlyList<CatalogBrandDto>> Handle(GetBrandsQuery request, CancellationToken cancellationToken) =>
        queries.GetBrandsAsync(cancellationToken);
}
