using BuildingBlocks.Application;
using Catalog.Application.Features.Brands;
using Catalog.Application.Features.Products;
using Catalog.Application.Features.Types;

namespace Catalog.Application.Abstractions;

// <summary> Read-side port. Implementations project straight to DTOs so no
// aggregate is materialised, and so Application needs no EF Core dependency. </summary>

public interface ICatalogQueries
{
    Task<PagedResult<CatalogItemDto>> GetProductsAsync(ProductFilter filter, CancellationToken cancellationToken);

    Task<CatalogItemDto?> GetProductAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CatalogBrandDto>> GetBrandsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<CatalogTypeDto>> GetTypesAsync(CancellationToken cancellationToken);

    Task<bool> BrandExistsAsync(int brandId, CancellationToken cancellationToken);

    Task<bool> TypeExistsAsync(int typeId, CancellationToken cancellationToken);
}
