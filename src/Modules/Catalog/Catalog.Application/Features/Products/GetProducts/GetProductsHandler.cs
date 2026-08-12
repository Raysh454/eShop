using BuildingBlocks.Application.CQRS;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.Application.Features.Products.GetProducts;

public class GetProductsHandler : IQueryHandler<GetProductsQuery, IEnumerable<CatalogItemDto>>
{
    public Task<IEnumerable<CatalogItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // For now, returning an empty list to compile.
        IEnumerable<CatalogItemDto> items = new List<CatalogItemDto>();
        return Task.FromResult(items);
    }
}
