using Catalog.Domain;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Catalog.Tests.Integration.Fixtures;

// <summary> Starts one SQL Server container for the whole test collection and
// applies the real migrations to it. Tests run against the schema the
// application actually ships, not a model built on the fly. </summary>

public sealed class CatalogDatabaseFixture : IAsyncLifetime
{
    // Pinned to the same tag the local compose setup uses, so a test run does
    // not pull a second multi-gigabyte image.
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public CatalogContext CreateContext() =>
        new(new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer(ConnectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", CatalogContext.Schema))
            .Options);

    // <summary> Brand and type names are uniquely indexed, so every test mints
    // its own rather than fighting over shared fixtures. </summary>
    public async Task<(int BrandId, int TypeId)> CreateClassificationAsync()
    {
        await using var context = CreateContext();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var brand = context.CatalogBrands.Add(new CatalogBrand($"Brand-{suffix}")).Entity;
        var type = context.CatalogTypes.Add(new CatalogType($"Type-{suffix}")).Entity;

        await context.SaveChangesAsync();

        return (brand.Id, type.Id);
    }
}
