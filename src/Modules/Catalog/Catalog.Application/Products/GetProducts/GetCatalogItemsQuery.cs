using BuildingBlocks.Application.CQRS;
using MediatR;
using System.Collections.Generic;

namespace Catalog.Application.Products.GetProducts;

public record GetCatalogItemsQuery() : IQuery<IEnumerable<CatalogItemDto>>;

public class GetCatalogItemsQueryHandler : IQueryHandler<GetCatalogItemsQuery, IEnumerable<CatalogItemDto>>
{
    public Task<IEnumerable<CatalogItemDto>> Handle(GetCatalogItemsQuery request, CancellationToken cancellationToken)
    {
        // For now, this is a placeholder implementation returning an empty list.
        IEnumerable<CatalogItemDto> items = new List<CatalogItemDto>();
        return Task.FromResult(items);
    }
}
