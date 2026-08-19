using BuildingBlocks.Application;
using BuildingBlocks.Application.CQRS;
using Catalog.Application.Abstractions;

namespace Catalog.Application.Features.Products.GetProducts;

public sealed class GetProductsHandler(ICatalogQueries queries)
    : IQueryHandler<GetProductsQuery, PagedResult<CatalogItemDto>>
{
    public Task<PagedResult<CatalogItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        queries.GetProductsAsync(
            new ProductFilter(request.Page, request.PageSize, request.BrandId, request.TypeId, request.Search),
            cancellationToken);
}
