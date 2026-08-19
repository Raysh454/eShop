namespace Catalog.Application.Features.Products;

public record CatalogItemDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string PictureFileName,
    string PictureUri,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    bool OnReorder,
    int CatalogTypeId,
    int CatalogBrandId
);
