using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Domain;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.CreateProduct;

public sealed class CreateProductHandler(
    ICatalogItemRepository repository,
    ICatalogQueries queries,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Checked up front so an unknown brand or type reads as a 404 rather
        // than surfacing as a foreign key violation from the database.
        if (!await queries.BrandExistsAsync(request.CatalogBrandId, cancellationToken))
            throw new NotFoundException(nameof(CatalogBrand), request.CatalogBrandId);

        if (!await queries.TypeExistsAsync(request.CatalogTypeId, cancellationToken))
            throw new NotFoundException(nameof(CatalogType), request.CatalogTypeId);

        var item = CatalogItem.Create(
            request.Name,
            request.Description,
            Money.From(request.Price, request.Currency),
            request.PictureFileName,
            request.PictureUri,
            request.CatalogTypeId,
            request.CatalogBrandId,
            request.AvailableStock,
            request.RestockThreshold,
            request.MaxStockThreshold);

        repository.Add(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
