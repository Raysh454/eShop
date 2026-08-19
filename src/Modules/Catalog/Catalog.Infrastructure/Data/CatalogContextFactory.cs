using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infrastructure.Data;

// <summary> Lets `dotnet ef` build the model straight from this project without
// starting the host. The connection string is only used to pick the provider's
// SQL dialect; no connection is opened when scaffolding a migration. </summary>

public sealed class CatalogContextFactory : IDesignTimeDbContextFactory<CatalogContext>
{
    private const string DesignTimeConnectionString =
        "Server=localhost,1433;Database=eShop;User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False";

    public CatalogContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CATALOG_CONNECTION_STRING")
            ?? DesignTimeConnectionString;

        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", CatalogContext.Schema))
            .Options;

        return new CatalogContext(options);
    }
}
