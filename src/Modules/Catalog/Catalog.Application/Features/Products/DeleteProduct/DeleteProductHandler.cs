using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.Features.Products.DeleteProduct;

public sealed class DeleteProductHandler(ICatalogItemRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogItem), request.Id);

        repository.Remove(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
