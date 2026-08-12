using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string PictureFileName,
    string PictureUri,
    int CatalogTypeId,
    int CatalogBrandId,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold
) : ICommand<int>;
