using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Catalog.Application.Features.Products;
using Catalog.Application.Features.Products.GetProducts;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Catalog-Products")]
public sealed class ProductsController(IMediator mediatr) : ControllerBase
{
    // <summary> List all products in catalog </summary>
    [HttpGet("/all")]
    [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllProducts(CancellationToken cancellationToken)
    {
        var result = await mediatr.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result);
    }
}