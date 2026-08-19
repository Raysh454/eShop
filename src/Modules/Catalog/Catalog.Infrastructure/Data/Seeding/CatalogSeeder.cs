using Catalog.Domain;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Data.Seeding;

// <summary> Seeds through the aggregate rather than through HasData, so seeded
// rows are subject to the same invariants as anything the API creates. </summary>

public static class CatalogSeeder
{
    public static async Task SeedAsync(CatalogContext context, CancellationToken cancellationToken = default)
    {
        if (await context.CatalogItems.AnyAsync(cancellationToken))
            return;

        var brands = new Dictionary<string, CatalogBrand>();
        foreach (var name in new[] { "Contoso", "Fabrikam", "Northwind" })
        {
            var brand = await context.CatalogBrands.FirstOrDefaultAsync(b => b.Brand == name, cancellationToken);
            brand ??= context.CatalogBrands.Add(new CatalogBrand(name)).Entity;
            brands[name] = brand;
        }

        var types = new Dictionary<string, CatalogType>();
        foreach (var name in new[] { "Peripherals", "Displays", "Apparel" })
        {
            var type = await context.CatalogTypes.FirstOrDefaultAsync(t => t.Type == name, cancellationToken);
            type ??= context.CatalogTypes.Add(new CatalogType(name)).Entity;
            types[name] = type;
        }

        // HiLo assigns identities on Add, so the brand and type ids below are
        // already populated even though nothing has been saved yet.
        await context.SaveChangesAsync(cancellationToken);

        context.CatalogItems.AddRange(
            CatalogItem.Create("Mechanical Keyboard", "Tenkeyless mechanical keyboard with hot-swappable switches",
                Money.From(129.99m), "keyboard.png", "https://images.example/keyboard.png",
                types["Peripherals"].Id, brands["Contoso"].Id, 40, 10, 100),

            CatalogItem.Create("Wireless Mouse", "Six-button wireless mouse with a rechargeable cell",
                Money.From(59.50m), "mouse.png", "https://images.example/mouse.png",
                types["Peripherals"].Id, brands["Contoso"].Id, 8, 10, 80),

            CatalogItem.Create("27in 4K Monitor", "27 inch 4K IPS display with USB-C power delivery",
                Money.From(449.00m), "monitor.png", "https://images.example/monitor.png",
                types["Displays"].Id, brands["Fabrikam"].Id, 12, 5, 40),

            CatalogItem.Create("Ultrawide Monitor", "34 inch curved ultrawide display",
                Money.From(729.00m), "ultrawide.png", "https://images.example/ultrawide.png",
                types["Displays"].Id, brands["Fabrikam"].Id, 3, 5, 25),

            CatalogItem.Create("Developer Hoodie", "Heavyweight cotton hoodie",
                Money.From(64.00m), "hoodie.png", "https://images.example/hoodie.png",
                types["Apparel"].Id, brands["Northwind"].Id, 120, 25, 200));

        await context.SaveChangesAsync(cancellationToken);
    }
}
