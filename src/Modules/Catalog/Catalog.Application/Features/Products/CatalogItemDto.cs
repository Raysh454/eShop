namespace Catalog.Application.Features.Products;

public record CatalogItemDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string PictureFileName,
    string PictureUri,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    int CatalogTypeId,
    int CatalogBrandId
);
