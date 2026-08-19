using BuildingBlocks.Domain;
using Catalog.Domain.Events;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain;

public class CatalogItem : AggregateRoot<int>
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 1000;
    public const int MaxPictureFileNameLength = 255;
    public const int MaxPictureUriLength = 1000;

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
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

    private CatalogItem(string name, string description, Money price, string pictureFileName, string pictureUri,
        int catalogTypeId, int catalogBrandId, int availableStock, int restockThreshold, int maxStockThreshold)
    {
        EnsureValidStock(availableStock, restockThreshold, maxStockThreshold);

        ApplyDetails(name, description, pictureFileName, pictureUri);
        Price = price ?? throw new CatalogDomainException("Price is required.");
        CatalogTypeId = EnsurePositiveId(catalogTypeId, nameof(catalogTypeId));
        CatalogBrandId = EnsurePositiveId(catalogBrandId, nameof(catalogBrandId));
        AvailableStock = availableStock;
        RestockThreshold = restockThreshold;
        MaxStockThreshold = maxStockThreshold;
        OnReorder = AvailableStock <= RestockThreshold;
    }

    public static CatalogItem Create(string name, string description, Money price, string pictureFileName, string pictureUri,
        int catalogTypeId, int catalogBrandId, int availableStock, int restockThreshold, int maxStockThreshold)
    {
        var item = new CatalogItem(name, description, price, pictureFileName, pictureUri,
            catalogTypeId, catalogBrandId, availableStock, restockThreshold, maxStockThreshold);
        item.AddDomainEvent(new ProductCreatedDomainEvent(item));
        return item;
    }

    // <summary> Updates the descriptive fields. Price and stock have their own
    // intent-revealing methods because they raise integration-visible events. </summary>
    public void ChangeDetails(string name, string description, string pictureFileName, string pictureUri) =>
        ApplyDetails(name, description, pictureFileName, pictureUri);

    public void ChangePrice(Money price)
    {
        if (price is null) throw new CatalogDomainException("Price is required.");
        if (price.Currency != Price.Currency)
            throw new CatalogDomainException($"Cannot price '{Name}' in {price.Currency}; the item is priced in {Price.Currency}.");
        if (price == Price) return;

        var oldPrice = Price;
        Price = price;
        AddDomainEvent(new ProductPriceChangedDomainEvent(this, oldPrice, price));
    }

    // <summary> Reserves up to <paramref name="quantityDesired"/> units and returns
    // how many were actually taken, which may be fewer than requested. </summary>
    public int RemoveStock(int quantityDesired)
    {
        if (quantityDesired <= 0)
            throw new CatalogDomainException("Quantity must be greater than zero.");
        if (AvailableStock == 0)
            throw new CatalogDomainException($"Product '{Name}' is out of stock.");

        var previousStock = AvailableStock;
        var removed = Math.Min(quantityDesired, AvailableStock);
        AvailableStock -= removed;
        OnReorder = AvailableStock <= RestockThreshold;
        AddDomainEvent(new ProductStockChangedDomainEvent(this, previousStock, AvailableStock));
        return removed;
    }

    // <summary> Restocks the item, clamped to the maximum threshold, and returns
    // how many units were actually added. </summary>
    public int AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new CatalogDomainException("Quantity must be greater than zero.");

        var previousStock = AvailableStock;
        AvailableStock = Math.Min(AvailableStock + quantity, MaxStockThreshold);
        var added = AvailableStock - previousStock;

        if (added == 0) return 0;

        OnReorder = AvailableStock <= RestockThreshold;
        AddDomainEvent(new ProductStockChangedDomainEvent(this, previousStock, AvailableStock));
        return added;
    }

    public void SetStockThresholds(int restockThreshold, int maxStockThreshold)
    {
        EnsureValidStock(AvailableStock, restockThreshold, maxStockThreshold);

        RestockThreshold = restockThreshold;
        MaxStockThreshold = maxStockThreshold;
        OnReorder = AvailableStock <= RestockThreshold;
    }

    // <summary> Validates every field before assigning any of them, so a rejected
    // update cannot leave the item half-changed. </summary>
    private void ApplyDetails(string name, string description, string pictureFileName, string pictureUri)
    {
        var validatedName = EnsureText(name, nameof(name), MaxNameLength);
        var validatedDescription = EnsureText(description, nameof(description), MaxDescriptionLength);
        var validatedPictureFileName = EnsureText(pictureFileName, nameof(pictureFileName), MaxPictureFileNameLength);
        var validatedPictureUri = EnsureText(pictureUri, nameof(pictureUri), MaxPictureUriLength);

        Name = validatedName;
        Description = validatedDescription;
        PictureFileName = validatedPictureFileName;
        PictureUri = validatedPictureUri;
    }

    private static string EnsureText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new CatalogDomainException($"{fieldName} is required.");

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new CatalogDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return trimmed;
    }

    private static void EnsureValidStock(int availableStock, int restockThreshold, int maxStockThreshold)
    {
        if (availableStock < 0)
            throw new CatalogDomainException("Available stock cannot be negative.");
        if (restockThreshold < 0)
            throw new CatalogDomainException("Restock threshold cannot be negative.");
        if (maxStockThreshold <= 0)
            throw new CatalogDomainException("Maximum stock threshold must be greater than zero.");
        if (restockThreshold > maxStockThreshold)
            throw new CatalogDomainException("Restock threshold cannot exceed maximum stock threshold.");
        if (availableStock > maxStockThreshold)
            throw new CatalogDomainException("Available stock cannot exceed maximum stock threshold.");
    }

    private static int EnsurePositiveId(int id, string fieldName) =>
        id > 0 ? id : throw new CatalogDomainException($"{fieldName} must be greater than zero.");
}
