using BuildingBlocks.Domain;
using Catalog.Domain.Events;

namespace Catalog.Domain;

public class CatalogItem : AggregateRoot<int>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string PictureFileName { get; private set; } = null!;
    public string PictureUri { get; private set; } = null!;
    public int CatalogTypeId { get; private set; }
    public CatalogType CatalogType { get; private set; } = null!;
    public int CatalogBrandId { get; private set; }
    public CatalogBrand CatalogBrand { get; private set; } = null!;
    public int AvailableStock { get; private set; }
    public int RestockThreshold { get; private set; }
    public int MaxStockThreshold { get; private set; }
    public bool OnReorder { get; private set; }

    protected CatalogItem() { }

    private CatalogItem(string name, string description, decimal price, string pictureFileName, string pictureUri,
        int catalogTypeId, int catalogBrandId, int availableStock, int restockThreshold, int maxStockThreshold)
    {
        EnsureValidDetails(name, description, price, pictureFileName, pictureUri);
        EnsureValidStock(availableStock, restockThreshold, maxStockThreshold);

        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        PictureFileName = pictureFileName.Trim();
        PictureUri = pictureUri.Trim();
        CatalogTypeId = EnsurePositiveId(catalogTypeId, nameof(catalogTypeId));
        CatalogBrandId = EnsurePositiveId(catalogBrandId, nameof(catalogBrandId));
        AvailableStock = availableStock;
        RestockThreshold = restockThreshold;
        MaxStockThreshold = maxStockThreshold;
        OnReorder = AvailableStock <= RestockThreshold;
    }

    public static CatalogItem Create(string name, string description, decimal price, string pictureFileName, string pictureUri,
        int catalogTypeId, int catalogBrandId, int availableStock, int restockThreshold, int maxStockThreshold)
    {
        var item = new CatalogItem(name, description, price, pictureFileName, pictureUri,
            catalogTypeId, catalogBrandId, availableStock, restockThreshold, maxStockThreshold);
        item.AddDomainEvent(new ProductCreatedDomainEvent(item.Name));
        return item;
    }

    public void ChangePrice(decimal price)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        Price = price;
    }

    public int RemoveStock(int quantityDesired)
    {
        if (AvailableStock == 0) throw new InvalidOperationException($"Product '{Name}' is out of stock.");
        if (quantityDesired <= 0) throw new ArgumentOutOfRangeException(nameof(quantityDesired), "Quantity must be greater than zero.");

        var removed = Math.Min(quantityDesired, AvailableStock);
        AvailableStock -= removed;
        OnReorder = AvailableStock <= RestockThreshold;
        return removed;
    }

    public int AddStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        var original = AvailableStock;
        AvailableStock = Math.Min(AvailableStock + quantity, MaxStockThreshold);
        OnReorder = AvailableStock <= RestockThreshold;
        return AvailableStock - original;
    }

    private static void EnsureValidDetails(string name, string description, decimal price, string pictureFileName, string pictureUri)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (string.IsNullOrWhiteSpace(pictureFileName)) throw new ArgumentException("Picture file name is required.", nameof(pictureFileName));
        if (string.IsNullOrWhiteSpace(pictureUri)) throw new ArgumentException("Picture URI is required.", nameof(pictureUri));
    }

    private static void EnsureValidStock(int availableStock, int restockThreshold, int maxStockThreshold)
    {
        if (availableStock < 0) throw new ArgumentOutOfRangeException(nameof(availableStock));
        if (restockThreshold < 0) throw new ArgumentOutOfRangeException(nameof(restockThreshold));
        if (maxStockThreshold <= 0) throw new ArgumentOutOfRangeException(nameof(maxStockThreshold));
        if (restockThreshold > maxStockThreshold) throw new ArgumentException("Restock threshold cannot exceed maximum stock threshold.", nameof(restockThreshold));
        if (availableStock > maxStockThreshold) throw new ArgumentException("Available stock cannot exceed maximum stock threshold.", nameof(availableStock));
    }

    private static int EnsurePositiveId(int id, string parameterName) =>
        id > 0 ? id : throw new ArgumentOutOfRangeException(parameterName, "Identifier must be greater than zero.");
}
