using BuildingBlocks.Application.CQRS;
using System.Collections.Generic;

namespace Catalog.Application.Features.Products.GetProducts;

public record GetProductsQuery() : IQuery<IEnumerable<CatalogItemDto>>;
