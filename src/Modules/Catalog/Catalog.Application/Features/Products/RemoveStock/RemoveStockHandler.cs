using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Domain;

namespace Catalog.Application.Features.Products.RemoveStock;

public sealed class RemoveStockHandler(ICatalogItemRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveStockCommand, StockChangedResponse>
{
    public async Task<StockChangedResponse> Handle(RemoveStockCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogItem), request.Id);

        var removed = item.RemoveStock(request.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new StockChangedResponse(item.Id, -removed, item.AvailableStock, item.OnReorder);
    }
}
