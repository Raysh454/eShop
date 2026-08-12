using Catalog.Domain;

namespace Catalog.Tests.Unit;

public class CatalogItemTests
{
    [Fact]
    public void Create_WhenStockIsAtRestockThreshold_MarksItemForReorder()
    {
        var item = CreateItem(availableStock: 5, restockThreshold: 5);
        Assert.True(item.OnReorder);
        Assert.Single(item.DomainEvents);
    }

    [Fact]
    public void RemoveStock_WhenStockFallsToThreshold_MarksItemForReorder()
    {
        var item = CreateItem(availableStock: 10, restockThreshold: 5);
        var removed = item.RemoveStock(5);
        Assert.Equal(5, removed);
        Assert.Equal(5, item.AvailableStock);
        Assert.True(item.OnReorder);
    }

    [Fact]
    public void AddStock_DoesNotExceedMaximumStockThreshold()
    {
        var item = CreateItem(availableStock: 8, maxStockThreshold: 10);
        var added = item.AddStock(5);
        Assert.Equal(2, added);
        Assert.Equal(10, item.AvailableStock);
    }

    [Fact]
    public void Create_WhenStockExceedsMaximum_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreateItem(availableStock: 11, maxStockThreshold: 10));
    }

    private static CatalogItem CreateItem(int availableStock = 10, int restockThreshold = 2, int maxStockThreshold = 20) =>
        CatalogItem.Create("Keyboard", "Mechanical keyboard", 99.99m, "keyboard.png", "https://images.example/keyboard.png", 1, 1,
            availableStock, restockThreshold, maxStockThreshold);
}
