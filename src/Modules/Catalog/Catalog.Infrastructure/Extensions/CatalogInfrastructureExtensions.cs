using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Data.Seeding;
using Catalog.Infrastructure.Repositories;
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

        services.AddScoped<ICatalogItemRepository, CatalogItemRepository>();
        services.AddScoped<ICatalogQueries, CatalogQueries>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    // <summary> Applies migrations and optionally seeds. Intended for development
    // and integration tests; production should run a migration bundle as a
    // separate deployment step rather than migrating on startup. </summary>
    public static async Task InitialiseCatalogAsync(
        this IServiceProvider services,
        bool seed = false,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();

        await context.Database.MigrateAsync(cancellationToken);

        if (seed)
            await CatalogSeeder.SeedAsync(context, cancellationToken);
    }
}
