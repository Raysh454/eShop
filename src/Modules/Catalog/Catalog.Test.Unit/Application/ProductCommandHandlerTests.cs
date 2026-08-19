using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Features.Products.AddStock;
using Catalog.Application.Features.Products.ChangeProductPrice;
using Catalog.Application.Features.Products.DeleteProduct;
using Catalog.Application.Features.Products.RemoveStock;
using Catalog.Application.Features.Products.UpdateProduct;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;
using Catalog.Tests.Unit.Domain;

namespace Catalog.Tests.Unit.Application;

public class ProductCommandHandlerTests
{
    private readonly FakeCatalogItemRepository _repository = new();

    [Fact]
    public async Task ChangePrice_UpdatesTheAggregateAndCommits()
    {
        var item = _repository.Seed(new CatalogItemBuilder().WithPrice(Money.From(10m)).BuildCreated());

        await new ChangeProductPriceHandler(_repository, _repository)
            .Handle(new ChangeProductPriceCommand(item.Id, 25m), CancellationToken.None);

        Assert.Equal(Money.From(25m), item.Price);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task ChangePrice_ForAMissingItem_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new ChangeProductPriceHandler(_repository, _repository)
                .Handle(new ChangeProductPriceCommand(404, 25m), CancellationToken.None));

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task ChangePrice_AcrossCurrencies_DoesNotCommit()
    {
        var item = _repository.Seed(new CatalogItemBuilder().WithPrice(Money.From(10m, "USD")).BuildCreated());

        await Assert.ThrowsAsync<CatalogDomainException>(() =>
            new ChangeProductPriceHandler(_repository, _repository)
                .Handle(new ChangeProductPriceCommand(item.Id, 25m, "EUR"), CancellationToken.None));

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task UpdateProduct_ChangesTheDetails()
    {
        var item = _repository.Seed(new CatalogItemBuilder().BuildCreated());

        await new UpdateProductHandler(_repository, _repository).Handle(
            new UpdateProductCommand(item.Id, "Trackball", "Ergonomic trackball", "ball.png", "https://images.example/ball.png"),
            CancellationToken.None);

        Assert.Equal("Trackball", item.Name);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task AddStock_ReportsTheQuantityActuallyAdded()
    {
        var item = _repository.Seed(new CatalogItemBuilder().WithStock(8, 2, 10).BuildCreated());

        var response = await new AddStockHandler(_repository, _repository)
            .Handle(new AddStockCommand(item.Id, 5), CancellationToken.None);

        // Asked for 5, clamped to the maximum threshold of 10.
        Assert.Equal(2, response.QuantityChanged);
        Assert.Equal(10, response.AvailableStock);
        Assert.False(response.OnReorder);
    }

    [Fact]
    public async Task RemoveStock_ReportsTheChangeAsNegative()
    {
        var item = _repository.Seed(new CatalogItemBuilder().WithStock(10, 8, 20).BuildCreated());

        var response = await new RemoveStockHandler(_repository, _repository)
            .Handle(new RemoveStockCommand(item.Id, 4), CancellationToken.None);

        Assert.Equal(-4, response.QuantityChanged);
        Assert.Equal(6, response.AvailableStock);
        Assert.True(response.OnReorder);
    }

    [Fact]
    public async Task RemoveStock_WhenOutOfStock_DoesNotCommit()
    {
        var item = _repository.Seed(new CatalogItemBuilder().WithStock(0, 2, 20).BuildCreated());

        await Assert.ThrowsAsync<CatalogDomainException>(() =>
            new RemoveStockHandler(_repository, _repository)
                .Handle(new RemoveStockCommand(item.Id, 1), CancellationToken.None));

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task DeleteProduct_RemovesTheItemAndCommits()
    {
        var item = _repository.Seed(new CatalogItemBuilder().BuildCreated());

        await new DeleteProductHandler(_repository, _repository)
            .Handle(new DeleteProductCommand(item.Id), CancellationToken.None);

        Assert.Same(item, Assert.Single(_repository.Removed));
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task DeleteProduct_ForAMissingItem_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new DeleteProductHandler(_repository, _repository)
                .Handle(new DeleteProductCommand(404), CancellationToken.None));
    }
}
