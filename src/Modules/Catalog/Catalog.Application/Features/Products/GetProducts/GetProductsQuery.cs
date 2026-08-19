using BuildingBlocks.Application;
using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Products.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    int? BrandId = null,
    int? TypeId = null,
    string? Search = null
) : IQuery<PagedResult<CatalogItemDto>>;
