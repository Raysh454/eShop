using BuildingBlocks.Domain;
using Catalog.Domain.Exceptions;

namespace Catalog.Domain;

public class CatalogBrand : Entity<int>
{
    public const int MaxBrandLength = 100;

    public string Brand { get; private set; } = null!;

    protected CatalogBrand() { }

    public CatalogBrand(string brand)
    {
        Rename(brand);
    }

    public void Rename(string brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new CatalogDomainException("Brand is required.");

        var trimmed = brand.Trim();

        if (trimmed.Length > MaxBrandLength)
            throw new CatalogDomainException($"Brand cannot exceed {MaxBrandLength} characters.");

        Brand = trimmed;
    }
}
