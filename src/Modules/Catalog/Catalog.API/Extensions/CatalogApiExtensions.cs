using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.Extensions;

// <summary> Registers the Catalog module's HTTP surface with the host. The host
// calls this instead of knowing about individual controllers, so the module can
// be extracted without touching composition code elsewhere. </summary>

public static class CatalogApiExtensions
{
    public static IMvcBuilder AddCatalogApi(this IMvcBuilder builder)
    {
        builder.AddApplicationPart(typeof(CatalogApiExtensions).Assembly);
        return builder;
    }
}
