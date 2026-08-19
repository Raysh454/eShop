using BuildingBlocks.Application;
using BuildingBlocks.Application.Exceptions;
using Catalog.Application.Features.Products;
using Catalog.Application.Features.Products.GetProduct;
using Catalog.Application.Features.Products.GetProducts;

namespace Catalog.Tests.Unit.Application;

public class ProductQueryHandlerTests
{
    private readonly FakeCatalogQueries _queries = new();

    [Fact]
    public async Task GetProducts_PassesPagingAndFiltersToTheReadPort()
    {
        await new GetProductsHandler(_queries).Handle(
            new GetProductsQuery(Page: 3, PageSize: 10, BrandId: 7, TypeId: 4, Search: "mouse"),
            CancellationToken.None);

        Assert.NotNull(_queries.LastFilter);
        var filter = _queries.LastFilter;
        Assert.Equal(3, filter.Page);
        Assert.Equal(10, filter.PageSize);
        Assert.Equal(7, filter.BrandId);
        Assert.Equal(4, filter.TypeId);
        Assert.Equal("mouse", filter.Search);
    }

    [Fact]
    public async Task GetProduct_ReturnsTheProjection()
    {
        _queries.Product = Dto(5);

        var result = await new GetProductHandler(_queries).Handle(new GetProductQuery(5), CancellationToken.None);

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task GetProduct_WhenMissing_ThrowsNotFound()
    {
        _queries.Product = null;

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetProductHandler(_queries).Handle(new GetProductQuery(404), CancellationToken.None));
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(10, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    public void PagedResult_ComputesTotalPages(int totalCount, int pageSize, int expected)
    {
        Assert.Equal(expected, new PagedResult<CatalogItemDto>([], 1, pageSize, totalCount).TotalPages);
    }

    [Fact]
    public void PagedResult_KnowsWhetherMorePagesExist()
    {
        var page = new PagedResult<CatalogItemDto>([Dto(1)], Page: 2, PageSize: 10, TotalCount: 35);

        Assert.True(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public void PagedResult_OnTheLastPage_HasNoNextPage()
    {
        var page = new PagedResult<CatalogItemDto>([Dto(1)], Page: 4, PageSize: 10, TotalCount: 35);

        Assert.False(page.HasNextPage);
    }

    private static CatalogItemDto Dto(int id) => new(
        id, "Keyboard", "Mechanical keyboard", 99.99m, "USD",
        "keyboard.png", "https://images.example/keyboard.png",
        10, 2, 20, false, 1, 1);
}
