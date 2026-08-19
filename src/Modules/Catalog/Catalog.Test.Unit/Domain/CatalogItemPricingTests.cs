using Catalog.Domain.Events;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Tests.Unit.Domain;

public class CatalogItemPricingTests
{
    [Fact]
    public void ChangePrice_UpdatesThePrice()
    {
        var item = new CatalogItemBuilder().WithPrice(Money.From(10m)).BuildCreated();

        item.ChangePrice(Money.From(12.50m));

        Assert.Equal(Money.From(12.50m), item.Price);
    }

    [Fact]
    public void ChangePrice_RaisesPriceChangedWithOldAndNewValues()
    {
        var item = new CatalogItemBuilder().WithPrice(Money.From(10m)).BuildCreated();

        item.ChangePrice(Money.From(12.50m));

        var changed = Assert.IsType<ProductPriceChangedDomainEvent>(Assert.Single(item.DomainEvents));
        Assert.Same(item, changed.Item);
        Assert.Equal(Money.From(10m), changed.OldPrice);
        Assert.Equal(Money.From(12.50m), changed.NewPrice);
    }

    [Fact]
    public void ChangePrice_ToTheSameValue_RaisesNoEvent()
    {
        var item = new CatalogItemBuilder().WithPrice(Money.From(10m)).BuildCreated();

        item.ChangePrice(Money.From(10.00m));

        Assert.Empty(item.DomainEvents);
    }

    [Fact]
    public void ChangePrice_ToADifferentCurrency_Throws()
    {
        var item = new CatalogItemBuilder().WithPrice(Money.From(10m, "USD")).BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.ChangePrice(Money.From(10m, "EUR")));
    }

    [Fact]
    public void ChangePrice_WithNull_Throws()
    {
        var item = new CatalogItemBuilder().BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.ChangePrice(null!));
    }

    [Fact]
    public void ChangePrice_WhenRejected_LeavesThePriceUnchanged()
    {
        var original = Money.From(10m, "USD");
        var item = new CatalogItemBuilder().WithPrice(original).BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.ChangePrice(Money.From(99m, "EUR")));

        Assert.Equal(original, item.Price);
        Assert.Empty(item.DomainEvents);
    }

    [Fact]
    public void ChangeDetails_UpdatesAndTrimsTextFields()
    {
        var item = new CatalogItemBuilder().BuildCreated();

        item.ChangeDetails("  Trackball  ", "  Ergonomic trackball  ", "  ball.png  ", "  https://images.example/ball.png  ");

        Assert.Equal("Trackball", item.Name);
        Assert.Equal("Ergonomic trackball", item.Description);
        Assert.Equal("ball.png", item.PictureFileName);
        Assert.Equal("https://images.example/ball.png", item.PictureUri);
    }

    [Fact]
    public void ChangeDetails_RaisesNoEvent()
    {
        var item = new CatalogItemBuilder().BuildCreated();

        item.ChangeDetails("Trackball", "Ergonomic trackball", "ball.png", "https://images.example/ball.png");

        Assert.Empty(item.DomainEvents);
    }

    [Fact]
    public void ChangeDetails_WithBlankName_Throws()
    {
        var item = new CatalogItemBuilder().WithName("Keyboard").BuildCreated();

        Assert.Throws<CatalogDomainException>(() =>
            item.ChangeDetails("  ", "Ergonomic trackball", "ball.png", "https://images.example/ball.png"));

        Assert.Equal("Keyboard", item.Name);
    }
}
