using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.Features.Products.UpdateProduct;

public sealed class UpdateProductHandler(ICatalogItemRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogItem), request.Id);

        item.ChangeDetails(request.Name, request.Description, request.PictureFileName, request.PictureUri);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
