using Catalog.Application.Features.Products;
using Catalog.Application.Features.Products.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/products")]
[Tags("Catalog-Products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    // <summary> List all products in the catalog. </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CatalogItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(products);
    }
}
