using BuildingBlocks.Application;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.Brands;
using Catalog.Application.Features.Products;
using Catalog.Application.Features.Types;

namespace Catalog.Tests.Unit.Application;

internal sealed class FakeCatalogQueries : ICatalogQueries
{
    public HashSet<int> BrandIds { get; } = [1];

    public HashSet<int> TypeIds { get; } = [1];

    public CatalogItemDto? Product { get; set; }

    public PagedResult<CatalogItemDto> Products { get; set; } = PagedResult<CatalogItemDto>.Empty(1, 20);

    public ProductFilter? LastFilter { get; private set; }

    public Task<PagedResult<CatalogItemDto>> GetProductsAsync(ProductFilter filter, CancellationToken cancellationToken)
    {
        LastFilter = filter;
        return Task.FromResult(Products);
    }

    public Task<CatalogItemDto?> GetProductAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(Product);

    public Task<IReadOnlyList<CatalogBrandDto>> GetBrandsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CatalogBrandDto>>([new CatalogBrandDto(1, "Contoso")]);

    public Task<IReadOnlyList<CatalogTypeDto>> GetTypesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CatalogTypeDto>>([new CatalogTypeDto(1, "Peripherals")]);

    public Task<bool> BrandExistsAsync(int brandId, CancellationToken cancellationToken) =>
        Task.FromResult(BrandIds.Contains(brandId));

    public Task<bool> TypeExistsAsync(int typeId, CancellationToken cancellationToken) =>
        Task.FromResult(TypeIds.Contains(typeId));
}
