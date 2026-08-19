using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Features.Products.CreateProduct;
using Catalog.Domain.ValueObjects;

namespace Catalog.Tests.Unit.Application;

public class CreateProductHandlerTests
{
    private readonly FakeCatalogItemRepository _repository = new();
    private readonly FakeCatalogQueries _queries = new();

    [Fact]
    public async Task Handle_AddsTheItemAndReturnsItsId()
    {
        var id = await Handler().Handle(Command(), CancellationToken.None);

        var added = Assert.Single(_repository.Added);
        Assert.Equal(added.Id, id);
        Assert.NotEqual(0, id);
    }

    [Fact]
    public async Task Handle_CommitsExactlyOnce()
    {
        await Handler().Handle(Command(), CancellationToken.None);

        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Handle_BuildsThePriceFromAmountAndCurrency()
    {
        await Handler().Handle(Command() with { Price = 12.50m, Currency = "eur" }, CancellationToken.None);

        Assert.Equal(Money.From(12.50m, "EUR"), Assert.Single(_repository.Added).Price);
    }

    [Fact]
    public async Task Handle_WhenBrandDoesNotExist_ThrowsNotFound()
    {
        _queries.BrandIds.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() => Handler().Handle(Command(), CancellationToken.None));
        Assert.Empty(_repository.Added);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenTypeDoesNotExist_ThrowsNotFound()
    {
        _queries.TypeIds.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() => Handler().Handle(Command(), CancellationToken.None));
        Assert.Empty(_repository.Added);
    }

    [Fact]
    public async Task Handle_RaisesProductCreatedOnTheNewItem()
    {
        await Handler().Handle(Command(), CancellationToken.None);

        Assert.Single(Assert.Single(_repository.Added).DomainEvents);
    }

    private CreateProductHandler Handler() => new(_repository, _queries, _repository);

    private static CreateProductCommand Command() => new(
        Name: "Keyboard",
        Description: "Mechanical keyboard",
        Price: 99.99m,
        PictureFileName: "keyboard.png",
        PictureUri: "https://images.example/keyboard.png",
        CatalogTypeId: 1,
        CatalogBrandId: 1,
        AvailableStock: 10,
        RestockThreshold: 2,
        MaxStockThreshold: 20);
}
