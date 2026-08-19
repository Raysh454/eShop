using Catalog.Domain;
using Catalog.Domain.ValueObjects;
using Catalog.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Integration.Persistence;

[Collection(DatabaseCollection.Name)]
public class CatalogItemPersistenceTests(CatalogDatabaseFixture fixture)
{
    [Fact]
    public async Task Saving_and_reloading_preserves_every_field()
    {
        // The regression guard for the mapping that used to ignore PictureUri:
        // the write succeeded and the value silently vanished.
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        var item = CatalogItem.Create(
            "Mechanical Keyboard", "Tenkeyless with hot-swappable switches",
            Money.From(129.99m, "EUR"), "keyboard.png", "https://images.example/keyboard.png",
            typeId, brandId, 40, 10, 100);

        int id;
        await using (var context = fixture.CreateContext())
        {
            context.CatalogItems.Add(item);
            await context.SaveChangesAsync();
            id = item.Id;
        }

        await using (var context = fixture.CreateContext())
        {
            var reloaded = await context.CatalogItems.SingleAsync(i => i.Id == id);

            Assert.Equal("Mechanical Keyboard", reloaded.Name);
            Assert.Equal("Tenkeyless with hot-swappable switches", reloaded.Description);
            Assert.Equal("keyboard.png", reloaded.PictureFileName);
            Assert.Equal("https://images.example/keyboard.png", reloaded.PictureUri);
            Assert.Equal(Money.From(129.99m, "EUR"), reloaded.Price);
            Assert.Equal(40, reloaded.AvailableStock);
            Assert.Equal(10, reloaded.RestockThreshold);
            Assert.Equal(100, reloaded.MaxStockThreshold);
            Assert.False(reloaded.OnReorder);
            Assert.Equal(brandId, reloaded.CatalogBrandId);
            Assert.Equal(typeId, reloaded.CatalogTypeId);
        }
    }

    [Fact]
    public async Task Identity_is_assigned_before_the_transaction_commits()
    {
        // HiLo assigns on Add, which is what lets a domain event carry the
        // aggregate and still resolve an Id when it is dispatched.
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        var item = Item(brandId, typeId);

        await using var context = fixture.CreateContext();
        context.CatalogItems.Add(item);

        Assert.NotEqual(0, item.Id);
    }

    [Fact]
    public async Task Price_is_stored_as_amount_and_currency_columns()
    {
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        var item = Item(brandId, typeId, Money.From(19.99m, "GBP"));

        await using var context = fixture.CreateContext();
        context.CatalogItems.Add(item);
        await context.SaveChangesAsync();

        var amounts = await context.Database
            .SqlQuery<decimal>($"SELECT Price AS Value FROM catalog.CatalogItem WHERE Id = {item.Id}")
            .ToListAsync();
        var currencies = await context.Database
            .SqlQuery<string>($"SELECT Currency AS Value FROM catalog.CatalogItem WHERE Id = {item.Id}")
            .ToListAsync();

        Assert.Equal(19.99m, Assert.Single(amounts));
        Assert.Equal("GBP", Assert.Single(currencies).Trim());
    }

    [Fact]
    public async Task Domain_events_are_not_persisted()
    {
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        var item = Item(brandId, typeId);

        await using var context = fixture.CreateContext();
        context.CatalogItems.Add(item);
        await context.SaveChangesAsync();

        // Create raised one; it must not have become a column or a table.
        Assert.Single(item.DomainEvents);
        Assert.DoesNotContain(
            context.Model.FindEntityType(typeof(CatalogItem))!.GetProperties(),
            property => property.Name.Contains("DomainEvent"));
    }

    [Fact]
    public async Task Duplicate_brand_names_are_rejected()
    {
        var name = $"Brand-{Guid.NewGuid():N}";

        await using (var context = fixture.CreateContext())
        {
            context.CatalogBrands.Add(new CatalogBrand(name));
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            context.CatalogBrands.Add(new CatalogBrand(name));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task Duplicate_type_names_are_rejected()
    {
        var name = $"Type-{Guid.NewGuid():N}";

        await using (var context = fixture.CreateContext())
        {
            context.CatalogTypes.Add(new CatalogType(name));
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            context.CatalogTypes.Add(new CatalogType(name));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task Concurrent_stock_changes_are_detected()
    {
        // Without the RowVersion column both removals would succeed against the
        // same starting quantity and the shop would oversell.
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        var item = Item(brandId, typeId);

        await using (var setup = fixture.CreateContext())
        {
            setup.CatalogItems.Add(item);
            await setup.SaveChangesAsync();
        }

        await using var first = fixture.CreateContext();
        await using var second = fixture.CreateContext();

        var firstCopy = await first.CatalogItems.SingleAsync(i => i.Id == item.Id);
        var secondCopy = await second.CatalogItems.SingleAsync(i => i.Id == item.Id);

        firstCopy.RemoveStock(5);
        await first.SaveChangesAsync();

        secondCopy.RemoveStock(5);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    private static CatalogItem Item(int brandId, int typeId, Money? price = null) => CatalogItem.Create(
        "Wireless Mouse", "Six-button wireless mouse",
        price ?? Money.From(59.50m), "mouse.png", "https://images.example/mouse.png",
        typeId, brandId, 40, 10, 100);
}
