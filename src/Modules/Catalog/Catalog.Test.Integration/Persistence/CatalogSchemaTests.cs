using Catalog.Infrastructure.Data;
using Catalog.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Integration.Persistence;

[Collection(DatabaseCollection.Name)]
public class CatalogSchemaTests(CatalogDatabaseFixture fixture)
{
    [Fact]
    public async Task Migrations_apply_cleanly_and_leave_nothing_pending()
    {
        await using var context = fixture.CreateContext();

        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task Model_matches_the_migrations()
    {
        // Catches the common slip of editing a configuration and forgetting to
        // scaffold the migration for it.
        await using var context = fixture.CreateContext();

        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task Catalog_tables_live_in_their_own_schema()
    {
        await using var context = fixture.CreateContext();

        var schemas = await context.Database
            .SqlQuery<string>($"SELECT TABLE_SCHEMA AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('CatalogItem', 'CatalogBrand', 'CatalogType')")
            .ToListAsync();

        Assert.Equal(3, schemas.Count);
        Assert.All(schemas, schema => Assert.Equal(CatalogContext.Schema, schema));
    }
}
