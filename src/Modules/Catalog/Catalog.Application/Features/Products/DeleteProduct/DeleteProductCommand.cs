using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Products.DeleteProduct;

public record DeleteProductCommand(int Id) : ICommand;
