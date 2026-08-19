using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Brands.GetBrands;

public record GetBrandsQuery() : IQuery<IReadOnlyList<CatalogBrandDto>>;
