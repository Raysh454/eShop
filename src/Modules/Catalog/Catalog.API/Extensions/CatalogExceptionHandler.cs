using Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Extensions;

// <summary> Maps a broken Catalog invariant to 400. Lives in the module rather
// than the host, so the host does not need to know any module's exception
// types; handlers are tried in registration order and this one declines
// anything that is not ours. </summary>

public sealed class CatalogExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not CatalogDomainException domainException)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Catalog rule violated",
                Detail = domainException.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1"
            }
        });
    }
}
