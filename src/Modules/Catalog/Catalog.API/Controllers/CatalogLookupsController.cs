using Catalog.Application.Features.Brands;
using Catalog.Application.Features.Brands.GetBrands;
using Catalog.Application.Features.Types;
using Catalog.Application.Features.Types.GetTypes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog")]
[Tags("Catalog-Lookups")]
public sealed class CatalogLookupsController(ISender sender) : ControllerBase
{
    // <summary> List catalog brands. </summary>
    [HttpGet("brands")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogBrandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogBrandDto>>> GetBrands(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetBrandsQuery(), cancellationToken));
    }

    // <summary> List catalog types. </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogTypeDto>>> GetTypes(CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetTypesQuery(), cancellationToken));
    }
}
