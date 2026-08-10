using BuildingBlocks.Domain;

namespace Catalog.Domain;

public class CatalogItem : AggregateRoot<int>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string PictureFileName { get; private set; }
    public string PictureUri { get; private set; }
    public int CatalogTypeId { get; private set; }
    public CatalogType CatalogType { get; private set; }
    public int CatalogBrandId { get; private set; }
    public CatalogBrand CatalogBrand { get; private set; }
    public int AvailableStock { get; private set; }
    public int RestockThreshold { get; private set; }
    public int MaxStockThreshold { get; private set; }

    public bool OnReorder { get; private set; }
    
    protected CatalogItem() { }

    public int RemoveStock(int quantityDesired)
    {
        if (AvailableStock == 0)
        {
            throw new Exception($"Empty stock, product item {Name} is depleted");
        }

        if (quantityDesired <= 0)
        {
            throw new Exception($"Item units desired should be greater than zero");
        }

        int removed = Math.Min(quantityDesired, AvailableStock);

        AvailableStock -= removed;

        return removed;
    }

    public int AddStock(int quantity)
    {
        int original = AvailableStock;
        if ((AvailableStock + quantity) > MaxStockThreshold)
        {
            AvailableStock += (MaxStockThreshold - AvailableStock);
        }
        else
        {
            AvailableStock += quantity;
        }

        OnReorder = false;

        return AvailableStock - original;
    }
}
