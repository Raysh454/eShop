using BuildingBlocks.Application.CQRS;
using Catalog.Domain.ValueObjects;

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
    int MaxStockThreshold,
    string Currency = Money.DefaultCurrency
) : ICommand<int>;
