using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure.Extensions;

// <summary> Registers Catalog's persistence. Takes the connection string rather
// than IConfiguration so the module never reaches into the host's config keys. </summary>

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", CatalogContext.Schema);
                sql.EnableRetryOnFailure();
            }));

        return services;
    }
}
