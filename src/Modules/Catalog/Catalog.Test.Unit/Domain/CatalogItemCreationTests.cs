using Catalog.Domain.Events;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Tests.Unit.Domain;

public class CatalogItemCreationTests
{
    [Fact]
    public void Create_AssignsAllDetails()
    {
        var price = Money.From(24.50m, "EUR");

        var item = new CatalogItemBuilder()
            .WithName("Mouse")
            .WithDescription("Wireless mouse")
            .WithPrice(price)
            .WithPictures("mouse.png", "https://images.example/mouse.png")
            .WithTypeId(3)
            .WithBrandId(7)
            .WithStock(15, 5, 30)
            .Build();

        Assert.Equal("Mouse", item.Name);
        Assert.Equal("Wireless mouse", item.Description);
        Assert.Equal(price, item.Price);
        Assert.Equal("mouse.png", item.PictureFileName);
        Assert.Equal("https://images.example/mouse.png", item.PictureUri);
        Assert.Equal(3, item.CatalogTypeId);
        Assert.Equal(7, item.CatalogBrandId);
        Assert.Equal(15, item.AvailableStock);
        Assert.Equal(5, item.RestockThreshold);
        Assert.Equal(30, item.MaxStockThreshold);
    }

    [Fact]
    public void Create_TrimsTextFields()
    {
        var item = new CatalogItemBuilder()
            .WithName("  Mouse  ")
            .WithDescription("  Wireless mouse  ")
            .WithPictures("  mouse.png  ", "  https://images.example/mouse.png  ")
            .Build();

        Assert.Equal("Mouse", item.Name);
        Assert.Equal("Wireless mouse", item.Description);
        Assert.Equal("mouse.png", item.PictureFileName);
        Assert.Equal("https://images.example/mouse.png", item.PictureUri);
    }

    [Fact]
    public void Create_RaisesProductCreatedCarryingTheAggregate()
    {
        var item = new CatalogItemBuilder().Build();

        var created = Assert.IsType<ProductCreatedDomainEvent>(Assert.Single(item.DomainEvents));
        Assert.Same(item, created.Item);
        Assert.NotEqual(Guid.Empty, created.EventId);
        Assert.NotEqual(default, created.OccurredOn);
    }

    [Fact]
    public void Create_WhenStockIsAtRestockThreshold_MarksItemForReorder()
    {
        var item = new CatalogItemBuilder().WithStock(5, 5, 20).Build();

        Assert.True(item.OnReorder);
    }

    [Fact]
    public void Create_WhenStockIsAboveRestockThreshold_DoesNotMarkItemForReorder()
    {
        var item = new CatalogItemBuilder().WithStock(6, 5, 20).Build();

        Assert.False(item.OnReorder);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name) =>
        AssertThrows(new CatalogItemBuilder().WithName(name));

    [Fact]
    public void Create_WithOverlongName_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithName(new string('a', Catalog.Domain.CatalogItem.MaxNameLength + 1)));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankDescription_Throws(string description) =>
        AssertThrows(new CatalogItemBuilder().WithDescription(description));

    [Fact]
    public void Create_WithOverlongDescription_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithDescription(new string('a', Catalog.Domain.CatalogItem.MaxDescriptionLength + 1)));

    [Fact]
    public void Create_WithBlankPictureFileName_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithPictures("  ", "https://images.example/x.png"));

    [Fact]
    public void Create_WithBlankPictureUri_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithPictures("x.png", "  "));

    [Fact]
    public void Create_WithNullPrice_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithPrice(null!));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveTypeId_Throws(int typeId) =>
        AssertThrows(new CatalogItemBuilder().WithTypeId(typeId));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveBrandId_Throws(int brandId) =>
        AssertThrows(new CatalogItemBuilder().WithBrandId(brandId));

    [Fact]
    public void Create_WithNegativeStock_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithStock(-1, 2, 20));

    [Fact]
    public void Create_WithNegativeRestockThreshold_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithStock(10, -1, 20));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithNonPositiveMaxStockThreshold_Throws(int max) =>
        AssertThrows(new CatalogItemBuilder().WithStock(0, 0, max));

    [Fact]
    public void Create_WhenRestockThresholdExceedsMaximum_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithStock(5, 21, 20));

    [Fact]
    public void Create_WhenStockExceedsMaximum_Throws() =>
        AssertThrows(new CatalogItemBuilder().WithStock(11, 2, 10));

    private static void AssertThrows(CatalogItemBuilder builder) =>
        Assert.Throws<CatalogDomainException>(() => builder.Build());
}
