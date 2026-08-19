using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Domain;

namespace Catalog.Application.Features.Products.AddStock;

public sealed class AddStockHandler(ICatalogItemRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<AddStockCommand, StockChangedResponse>
{
    public async Task<StockChangedResponse> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogItem), request.Id);

        var added = item.AddStock(request.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new StockChangedResponse(item.Id, added, item.AvailableStock, item.OnReorder);
    }
}
