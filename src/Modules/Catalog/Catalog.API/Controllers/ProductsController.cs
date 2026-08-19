using BuildingBlocks.Application;
using Catalog.Application.Features.Products;
using Catalog.Application.Features.Products.AddStock;
using Catalog.Application.Features.Products.ChangeProductPrice;
using Catalog.Application.Features.Products.CreateProduct;
using Catalog.Application.Features.Products.DeleteProduct;
using Catalog.Application.Features.Products.GetProduct;
using Catalog.Application.Features.Products.GetProducts;
using Catalog.Application.Features.Products.RemoveStock;
using Catalog.Application.Features.Products.UpdateProduct;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/products")]
[Tags("Catalog-Products")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    // <summary> List products, optionally filtered by brand, type or name. </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CatalogItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> GetProducts(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    // <summary> Fetch a single product. </summary>
    [HttpGet("{id:int}", Name = nameof(GetProduct))]
    [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CatalogItemDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetProductQuery(id), cancellationToken));
    }

    // <summary> Create a product. </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CatalogItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        var created = await sender.Send(new GetProductQuery(id), cancellationToken);

        return CreatedAtRoute(nameof(GetProduct), new { id }, created);
    }

    // <summary> Replace a product's descriptive fields. Price and stock have
    // their own endpoints because they are separately meaningful operations. </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateProductCommand(id, request.Name, request.Description, request.PictureFileName, request.PictureUri),
            cancellationToken);

        return NoContent();
    }

    // <summary> Change a product's price. </summary>
    [HttpPut("{id:int}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePrice(int id, ChangeProductPriceRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ChangeProductPriceCommand(id, request.Price, request.Currency), cancellationToken);

        return NoContent();
    }

    // <summary> Restock a product. The response reports the quantity actually
    // added, which is clamped to the maximum stock threshold. </summary>
    [HttpPost("{id:int}/stock/add")]
    [ProducesResponseType(typeof(StockChangedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockChangedResponse>> AddStock(int id, StockRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new AddStockCommand(id, request.Quantity), cancellationToken));
    }

    // <summary> Take stock off a product. The response reports the quantity
    // actually removed, which is clamped to what was available. </summary>
    [HttpPost("{id:int}/stock/remove")]
    [ProducesResponseType(typeof(StockChangedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockChangedResponse>> RemoveStock(int id, StockRequest request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new RemoveStockCommand(id, request.Quantity), cancellationToken));
    }

    // <summary> Delete a product. </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProductCommand(id), cancellationToken);

        return NoContent();
    }
}

// Request bodies for routes that take the id from the path, so the id cannot be
// specified twice and disagree with itself.
public record UpdateProductRequest(string Name, string Description, string PictureFileName, string PictureUri);

public record ChangeProductPriceRequest(decimal Price, string Currency = "USD");

public record StockRequest(int Quantity);
