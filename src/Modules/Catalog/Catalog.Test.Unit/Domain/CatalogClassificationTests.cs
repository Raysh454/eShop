using Catalog.Domain;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Unit.Domain;

public class CatalogClassificationTests
{
    [Fact]
    public void Brand_TrimsItsName()
    {
        Assert.Equal("Logitech", new CatalogBrand("  Logitech  ").Brand);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Brand_WithBlankName_Throws(string brand)
    {
        Assert.Throws<CatalogDomainException>(() => new CatalogBrand(brand));
    }

    [Fact]
    public void Brand_WithOverlongName_Throws()
    {
        Assert.Throws<CatalogDomainException>(() => new CatalogBrand(new string('a', CatalogBrand.MaxBrandLength + 1)));
    }

    [Fact]
    public void Brand_Rename_ReplacesTheName()
    {
        var brand = new CatalogBrand("Logitech");

        brand.Rename("Logitech G");

        Assert.Equal("Logitech G", brand.Brand);
    }

    [Fact]
    public void Type_TrimsItsName()
    {
        Assert.Equal("Peripherals", new CatalogType("  Peripherals  ").Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Type_WithBlankName_Throws(string type)
    {
        Assert.Throws<CatalogDomainException>(() => new CatalogType(type));
    }

    [Fact]
    public void Type_WithOverlongName_Throws()
    {
        Assert.Throws<CatalogDomainException>(() => new CatalogType(new string('a', CatalogType.MaxTypeLength + 1)));
    }

    // Entity identity semantics, exercised through a concrete entity rather than
    // a separate BuildingBlocks test project.

    [Fact]
    public void TransientEntities_AreNeverEqual()
    {
        // Both have Id 0 until persisted; treating them as equal would collapse
        // distinct unsaved brands into one another in sets and change tracking.
        Assert.NotEqual(new CatalogBrand("Logitech"), new CatalogBrand("Logitech"));
    }

    [Fact]
    public void SameInstance_IsEqualToItself()
    {
        var brand = new CatalogBrand("Logitech");

        Assert.Equal(brand, brand);
    }

    [Fact]
    public void EntitiesOfDifferentTypes_AreNotEqual()
    {
        Assert.False(new CatalogBrand("Peripherals").Equals(new CatalogType("Peripherals")));
    }
}
