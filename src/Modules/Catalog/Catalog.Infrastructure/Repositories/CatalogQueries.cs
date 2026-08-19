using System.Linq.Expressions;
using BuildingBlocks.Application;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.Brands;
using Catalog.Application.Features.Products;
using Catalog.Application.Features.Types;
using Catalog.Domain;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

// <summary> Read side. Every query projects to a DTO inside the SQL query, so
// no aggregate is materialised and no change tracking is set up for reads. </summary>

public sealed class CatalogQueries(CatalogContext context) : ICatalogQueries
{
    private static readonly Expression<Func<CatalogItem, CatalogItemDto>> ToDto = item => new CatalogItemDto(
        item.Id,
        item.Name,
        item.Description,
        item.Price.Amount,
        item.Price.Currency,
        item.PictureFileName,
        item.PictureUri,
        item.AvailableStock,
        item.RestockThreshold,
        item.MaxStockThreshold,
        item.OnReorder,
        item.CatalogTypeId,
        item.CatalogBrandId);

    public async Task<PagedResult<CatalogItemDto>> GetProductsAsync(ProductFilter filter, CancellationToken cancellationToken)
    {
        var query = context.CatalogItems.AsNoTracking();

        if (filter.BrandId is { } brandId)
            query = query.Where(item => item.CatalogBrandId == brandId);

        if (filter.TypeId is { } typeId)
            query = query.Where(item => item.CatalogTypeId == typeId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(item => item.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            return PagedResult<CatalogItemDto>.Empty(filter.Page, filter.PageSize);

        // Ordered by Id as a tiebreak: paging over a non-unique sort key can
        // otherwise repeat or skip rows between pages.
        var items = await query
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(ToDto)
            .ToListAsync(cancellationToken);

        return new PagedResult<CatalogItemDto>(items, filter.Page, filter.PageSize, totalCount);
    }

    public Task<CatalogItemDto?> GetProductAsync(int id, CancellationToken cancellationToken) =>
        context.CatalogItems
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogBrandDto>> GetBrandsAsync(CancellationToken cancellationToken) =>
        await context.CatalogBrands
            .AsNoTracking()
            .OrderBy(brand => brand.Brand)
            .Select(brand => new CatalogBrandDto(brand.Id, brand.Brand))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogTypeDto>> GetTypesAsync(CancellationToken cancellationToken) =>
        await context.CatalogTypes
            .AsNoTracking()
            .OrderBy(type => type.Type)
            .Select(type => new CatalogTypeDto(type.Id, type.Type))
            .ToListAsync(cancellationToken);

    public Task<bool> BrandExistsAsync(int brandId, CancellationToken cancellationToken) =>
        context.CatalogBrands.AsNoTracking().AnyAsync(brand => brand.Id == brandId, cancellationToken);

    public Task<bool> TypeExistsAsync(int typeId, CancellationToken cancellationToken) =>
        context.CatalogTypes.AsNoTracking().AnyAsync(type => type.Id == typeId, cancellationToken);
}
