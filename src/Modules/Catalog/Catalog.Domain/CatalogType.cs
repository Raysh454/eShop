using BuildingBlocks.Domain;
using Catalog.Domain.Exceptions;

namespace Catalog.Domain;

public class CatalogType : Entity<int>
{
    public const int MaxTypeLength = 100;

    public string Type { get; private set; } = null!;

    protected CatalogType() { }

    public CatalogType(string type)
    {
        Rename(type);
    }

    public void Rename(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new CatalogDomainException("Type is required.");

        var trimmed = type.Trim();

        if (trimmed.Length > MaxTypeLength)
            throw new CatalogDomainException($"Type cannot exceed {MaxTypeLength} characters.");

        Type = trimmed;
    }
}
