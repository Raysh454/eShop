using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Products.RemoveStock;

public record RemoveStockCommand(int Id, int Quantity) : ICommand<StockChangedResponse>;
