using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    int Id,
    string Name,
    string Description,
    string PictureFileName,
    string PictureUri
) : ICommand;
