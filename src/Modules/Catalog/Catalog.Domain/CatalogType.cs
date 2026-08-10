using BuildingBlocks.Domain;

namespace Catalog.Domain;

public class CatalogType : Entity<int>
{
    public string Type { get; private set; }

    protected CatalogType() { }

    public CatalogType(string type)
    {
        Type = type;
    }
}
