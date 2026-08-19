using BuildingBlocks.Application.CQRS;

namespace Catalog.Application.Features.Products.GetProduct;

public record GetProductQuery(int Id) : IQuery<CatalogItemDto>;
