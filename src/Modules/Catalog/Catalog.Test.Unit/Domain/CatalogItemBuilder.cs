using Catalog.Domain;
using Catalog.Domain.ValueObjects;

namespace Catalog.Tests.Unit.Domain;

// <summary> Keeps each test's arrange step to the one or two values it actually
// cares about, so a changed default never silently invalidates a test's intent. </summary>

internal sealed class CatalogItemBuilder
{
    private string _name = "Keyboard";
    private string _description = "Mechanical keyboard";
    private Money _price = Money.From(99.99m);
    private string _pictureFileName = "keyboard.png";
    private string _pictureUri = "https://images.example/keyboard.png";
    private int _catalogTypeId = 1;
    private int _catalogBrandId = 1;
    private int _availableStock = 10;
    private int _restockThreshold = 2;
    private int _maxStockThreshold = 20;

    public CatalogItemBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CatalogItemBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CatalogItemBuilder WithPrice(Money price)
    {
        _price = price;
        return this;
    }

    public CatalogItemBuilder WithPictures(string fileName, string uri)
    {
        _pictureFileName = fileName;
        _pictureUri = uri;
        return this;
    }

    public CatalogItemBuilder WithTypeId(int catalogTypeId)
    {
        _catalogTypeId = catalogTypeId;
        return this;
    }

    public CatalogItemBuilder WithBrandId(int catalogBrandId)
    {
        _catalogBrandId = catalogBrandId;
        return this;
    }

    public CatalogItemBuilder WithStock(int availableStock, int restockThreshold, int maxStockThreshold)
    {
        _availableStock = availableStock;
        _restockThreshold = restockThreshold;
        _maxStockThreshold = maxStockThreshold;
        return this;
    }

    public CatalogItem Build() => CatalogItem.Create(
        _name, _description, _price, _pictureFileName, _pictureUri,
        _catalogTypeId, _catalogBrandId, _availableStock, _restockThreshold, _maxStockThreshold);

    // <summary> Builds an item with the creation event already drained, so a test
    // asserting on a later event does not have to skip past it. </summary>
    public CatalogItem BuildCreated()
    {
        var item = Build();
        item.ClearDomainEvents();
        return item;
    }
}
