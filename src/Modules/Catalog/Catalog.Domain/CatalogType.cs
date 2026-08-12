using BuildingBlocks.Domain;

namespace Catalog.Domain;

public class CatalogType : Entity<int>
{
    public string Type { get; private set; } = null!;

    protected CatalogType() { }

    public CatalogType(string type)
    {
        Type = string.IsNullOrWhiteSpace(type) ? throw new ArgumentException("Type is required.", nameof(type)) : type.Trim();
    }
}
