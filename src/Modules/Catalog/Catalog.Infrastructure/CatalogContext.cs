namespace Catalog.Infrastructure;

using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions options) : base(options) 
    {
    }

    public DbSet<CatalogItem> CatalogItems { get; set; }
    public DbSet<CatalogBrand> CatalogBrands { get; set; }
    public DbSet<CatalogType> CatalogTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CatalogContext).Assembly);
    }
}
