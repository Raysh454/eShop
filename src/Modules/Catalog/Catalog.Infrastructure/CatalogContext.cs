namespace Catalog.Infrastructure;

using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions options) : base(options) 
    {
    }

    DbSet<CatalogItem> CatalogItems { get; set; }
    DbSet<CatalogBrand> CatalogBrands { get; set; }
    DbSet<CatalogType> CatalogTypes { get; set; }

}
