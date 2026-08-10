using BuildingBlocks.Domain;

namespace Catalog.Domain;

public class CatalogBrand : Entity<int>
{
    public string Brand { get; private set; }

    protected CatalogBrand() { }

    public CatalogBrand(string brand)
    {
        Brand = brand;
    }
}
