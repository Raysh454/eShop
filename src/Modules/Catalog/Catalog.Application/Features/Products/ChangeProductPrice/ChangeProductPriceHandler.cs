using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Domain;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Features.Products.ChangeProductPrice;

public sealed class ChangeProductPriceHandler(ICatalogItemRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeProductPriceCommand>
{
    public async Task<Unit> Handle(ChangeProductPriceCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogItem), request.Id);

        item.ChangePrice(Money.From(request.Price, request.Currency));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
