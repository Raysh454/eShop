using Catalog.Domain.Events;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Unit.Domain;

public class CatalogItemStockTests
{
    [Fact]
    public void RemoveStock_RemovesTheRequestedQuantity()
    {
        var item = new CatalogItemBuilder().WithStock(10, 2, 20).BuildCreated();

        var removed = item.RemoveStock(4);

        Assert.Equal(4, removed);
        Assert.Equal(6, item.AvailableStock);
    }

    [Fact]
    public void RemoveStock_WhenRequestExceedsAvailable_RemovesOnlyWhatIsLeft()
    {
        var item = new CatalogItemBuilder().WithStock(3, 2, 20).BuildCreated();

        var removed = item.RemoveStock(10);

        Assert.Equal(3, removed);
        Assert.Equal(0, item.AvailableStock);
    }

    [Fact]
    public void RemoveStock_WhenStockFallsToThreshold_MarksItemForReorder()
    {
        var item = new CatalogItemBuilder().WithStock(10, 5, 20).BuildCreated();

        item.RemoveStock(5);

        Assert.Equal(5, item.AvailableStock);
        Assert.True(item.OnReorder);
    }

    [Fact]
    public void RemoveStock_RaisesStockChangedWithBeforeAndAfterQuantities()
    {
        var item = new CatalogItemBuilder().WithStock(10, 2, 20).BuildCreated();

        item.RemoveStock(4);

        var changed = Assert.IsType<ProductStockChangedDomainEvent>(Assert.Single(item.DomainEvents));
        Assert.Same(item, changed.Item);
        Assert.Equal(10, changed.PreviousStock);
        Assert.Equal(6, changed.NewStock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RemoveStock_WithNonPositiveQuantity_Throws(int quantity)
    {
        var item = new CatalogItemBuilder().BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.RemoveStock(quantity));
        Assert.Empty(item.DomainEvents);
    }

    [Fact]
    public void RemoveStock_WhenOutOfStock_Throws()
    {
        var item = new CatalogItemBuilder().WithStock(0, 2, 20).BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.RemoveStock(1));
    }

    [Fact]
    public void AddStock_IncreasesAvailableStock()
    {
        var item = new CatalogItemBuilder().WithStock(5, 2, 20).BuildCreated();

        var added = item.AddStock(10);

        Assert.Equal(10, added);
        Assert.Equal(15, item.AvailableStock);
    }

    [Fact]
    public void AddStock_DoesNotExceedMaximumStockThreshold()
    {
        var item = new CatalogItemBuilder().WithStock(8, 2, 10).BuildCreated();

        var added = item.AddStock(5);

        Assert.Equal(2, added);
        Assert.Equal(10, item.AvailableStock);
    }

    [Fact]
    public void AddStock_WhenAlreadyAtMaximum_AddsNothingAndRaisesNoEvent()
    {
        var item = new CatalogItemBuilder().WithStock(10, 2, 10).BuildCreated();

        var added = item.AddStock(5);

        Assert.Equal(0, added);
        Assert.Equal(10, item.AvailableStock);
        Assert.Empty(item.DomainEvents);
    }

    [Fact]
    public void AddStock_WhenStockRisesAboveThreshold_ClearsReorderFlag()
    {
        var item = new CatalogItemBuilder().WithStock(2, 5, 20).BuildCreated();
        Assert.True(item.OnReorder);

        item.AddStock(10);

        Assert.False(item.OnReorder);
    }

    [Fact]
    public void AddStock_RaisesStockChangedWithBeforeAndAfterQuantities()
    {
        var item = new CatalogItemBuilder().WithStock(5, 2, 20).BuildCreated();

        item.AddStock(3);

        var changed = Assert.IsType<ProductStockChangedDomainEvent>(Assert.Single(item.DomainEvents));
        Assert.Equal(5, changed.PreviousStock);
        Assert.Equal(8, changed.NewStock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddStock_WithNonPositiveQuantity_Throws(int quantity)
    {
        var item = new CatalogItemBuilder().BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.AddStock(quantity));
    }

    [Fact]
    public void SetStockThresholds_UpdatesThresholdsAndRecomputesReorderFlag()
    {
        var item = new CatalogItemBuilder().WithStock(6, 2, 20).BuildCreated();
        Assert.False(item.OnReorder);

        item.SetStockThresholds(restockThreshold: 8, maxStockThreshold: 30);

        Assert.Equal(8, item.RestockThreshold);
        Assert.Equal(30, item.MaxStockThreshold);
        Assert.True(item.OnReorder);
    }

    [Fact]
    public void SetStockThresholds_WhenRestockExceedsMaximum_Throws()
    {
        var item = new CatalogItemBuilder().BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.SetStockThresholds(31, 30));
    }

    [Fact]
    public void SetStockThresholds_WhenCurrentStockWouldExceedNewMaximum_Throws()
    {
        var item = new CatalogItemBuilder().WithStock(20, 2, 20).BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.SetStockThresholds(2, 10));
    }

    [Fact]
    public void SetStockThresholds_WhenRejected_LeavesThresholdsUnchanged()
    {
        var item = new CatalogItemBuilder().WithStock(20, 2, 20).BuildCreated();

        Assert.Throws<CatalogDomainException>(() => item.SetStockThresholds(2, 10));

        Assert.Equal(2, item.RestockThreshold);
        Assert.Equal(20, item.MaxStockThreshold);
    }
}
