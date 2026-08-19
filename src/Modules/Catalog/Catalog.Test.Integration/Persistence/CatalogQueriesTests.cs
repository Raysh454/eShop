using Catalog.Application.Abstractions;
using Catalog.Domain;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Repositories;
using Catalog.Tests.Integration.Fixtures;

namespace Catalog.Tests.Integration.Persistence;

[Collection(DatabaseCollection.Name)]
public class CatalogQueriesTests(CatalogDatabaseFixture fixture)
{
    [Fact]
    public async Task Filtering_by_brand_returns_only_that_brand()
    {
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        await SeedAsync(brandId, typeId, "Alpha", "Beta", "Gamma");

        await using var context = fixture.CreateContext();
        var result = await new CatalogQueries(context)
            .GetProductsAsync(new ProductFilter(1, 20, brandId, null, null), CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal(brandId, item.CatalogBrandId));
    }

    [Fact]
    public async Task Paging_splits_the_result_and_reports_the_total()
    {
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        await SeedAsync(brandId, typeId, "Alpha", "Beta", "Gamma", "Delta", "Epsilon");

        await using var context = fixture.CreateContext();
        var queries = new CatalogQueries(context);

        var first = await queries.GetProductsAsync(new ProductFilter(1, 2, brandId, null, null), CancellationToken.None);
        var second = await queries.GetProductsAsync(new ProductFilter(2, 2, brandId, null, null), CancellationToken.None);

        Assert.Equal(5, first.TotalCount);
        Assert.Equal(3, first.TotalPages);
        Assert.Equal(2, first.Items.Count);
        Assert.True(first.HasNextPage);
        Assert.False(first.HasPreviousPage);

        // Pages must not overlap; a non-unique sort key without a tiebreak
        // would let the same row appear on both.
        Assert.Empty(first.Items.Select(i => i.Id).Intersect(second.Items.Select(i => i.Id)));
    }

    [Fact]
    public async Task Search_matches_on_name()
    {
        var (brandId, typeId) = await fixture.CreateClassificationAsync();
        await SeedAsync(brandId, typeId, "Widget", "Sprocket");

        await using var context = fixture.CreateContext();
        var result = await new CatalogQueries(context)
            .GetProductsAsync(new ProductFilter(1, 20, brandId, null, "Sprock"), CancellationToken.None);

        Assert.Equal("Sprocket", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task Empty_result_still_reports_its_paging_position()
    {
        await using var context = fixture.CreateContext();
        var result = await new CatalogQueries(context)
            .GetProductsAsync(new ProductFilter(2, 25, BrandId: int.MaxValue, null, null), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(25, result.PageSize);
    }

    [Fact]
    public async Task Missing_product_reads_as_null()
    {
        await using var context = fixture.CreateContext();

        Assert.Null(await new CatalogQueries(context).GetProductAsync(int.MaxValue, CancellationToken.None));
    }

    [Fact]
    public async Task Brand_and_type_existence_checks_agree_with_the_data()
    {
        var (brandId, typeId) = await fixture.CreateClassificationAsync();

        await using var context = fixture.CreateContext();
        var queries = new CatalogQueries(context);

        Assert.True(await queries.BrandExistsAsync(brandId, CancellationToken.None));
        Assert.True(await queries.TypeExistsAsync(typeId, CancellationToken.None));
        Assert.False(await queries.BrandExistsAsync(int.MaxValue, CancellationToken.None));
        Assert.False(await queries.TypeExistsAsync(int.MaxValue, CancellationToken.None));
    }

    private async Task SeedAsync(int brandId, int typeId, params string[] names)
    {
        await using var context = fixture.CreateContext();

        foreach (var name in names)
        {
            context.CatalogItems.Add(CatalogItem.Create(
                name, $"{name} description", Money.From(10m), "p.png", "https://images.example/p.png",
                typeId, brandId, 10, 2, 50));
        }

        await context.SaveChangesAsync();
    }
}
