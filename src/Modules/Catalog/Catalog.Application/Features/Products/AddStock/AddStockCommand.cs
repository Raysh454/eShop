using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Products.AddStock;

public record AddStockCommand(int Id, int Quantity) : ICommand<StockChangedResponse>;
