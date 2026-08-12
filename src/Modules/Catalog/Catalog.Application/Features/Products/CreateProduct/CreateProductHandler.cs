using BuildingBlocks.Application.CQRS;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.Application.Features.Products.CreateProduct;

public class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    public Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Create CatalogItem entity
        // 2. Save to database
        // 3. Return the ID

        return Task.FromResult(0);
    }
}
