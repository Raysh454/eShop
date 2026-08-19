using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Abstractions;
using Catalog.Domain;

namespace Catalog.Application.Features.Products.GetProduct;

public sealed class GetProductHandler(ICatalogQueries queries)
    : IQueryHandler<GetProductQuery, CatalogItemDto>
{
    public async Task<CatalogItemDto> Handle(GetProductQuery request, CancellationToken cancellationToken) =>
        await queries.GetProductAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogItem), request.Id);
}
