using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application;
using Catalog.Application.Features.Products;
using Catalog.Application.Features.Products.CreateProduct;
using Catalog.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Tests.Integration.Api;

// <summary> Drives the real host end to end: routing, model binding, the
// MediatR pipeline including validation, EF Core, and the exception handlers. </summary>

[Collection(DatabaseCollection.Name)]
public class ProductsEndpointTests(CatalogDatabaseFixture fixture) : IAsyncLifetime
{
    private const string Products = "/api/catalog/products";

    private CatalogApiFactory _factory = null!;
    private HttpClient _client = null!;
    private int _brandId;
    private int _typeId;

    public async Task InitializeAsync()
    {
        _factory = new CatalogApiFactory(fixture.ConnectionString);
        _client = _factory.CreateClient();
        (_brandId, _typeId) = await fixture.CreateClassificationAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return _factory.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task Create_returns_201_with_a_location_that_resolves()
    {
        var response = await _client.PostAsJsonAsync(Products, Command());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var followed = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, followed.StatusCode);
    }

    [Fact]
    public async Task Created_product_round_trips_through_the_api()
    {
        var created = await CreateAsync(Command() with { Name = "Webcam", Price = 89.95m, Currency = "EUR" });

        var fetched = await _client.GetFromJsonAsync<CatalogItemDto>($"{Products}/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal("Webcam", fetched.Name);
        Assert.Equal(89.95m, fetched.Price);
        Assert.Equal("EUR", fetched.Currency);
        Assert.Equal("https://images.example/p.png", fetched.PictureUri);
    }

    [Fact]
    public async Task List_is_paged_and_filterable()
    {
        foreach (var name in new[] { "Alpha", "Beta", "Gamma" })
            await CreateAsync(Command() with { Name = name });

        var page = await _client.GetFromJsonAsync<PagedResult<CatalogItemDto>>(
            $"{Products}?page=1&pageSize=2&brandId={_brandId}");

        Assert.NotNull(page);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public async Task Invalid_page_size_is_rejected()
    {
        var response = await _client.GetAsync($"{Products}?pageSize=5000");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Price_change_persists()
    {
        var created = await CreateAsync(Command());

        var response = await _client.PutAsJsonAsync($"{Products}/{created.Id}/price", new { price = 42.50m, currency = "USD" });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var fetched = await _client.GetFromJsonAsync<CatalogItemDto>($"{Products}/{created.Id}");
        Assert.Equal(42.50m, fetched!.Price);
    }

    [Fact]
    public async Task Removing_more_stock_than_available_takes_only_what_is_there()
    {
        var created = await CreateAsync(Command() with { AvailableStock = 4, RestockThreshold = 2, MaxStockThreshold = 50 });

        var response = await _client.PostAsJsonAsync($"{Products}/{created.Id}/stock/remove", new { quantity = 10 });
        var result = await response.Content.ReadFromJsonAsync<StockChangedResponse>();

        Assert.Equal(-4, result!.QuantityChanged);
        Assert.Equal(0, result.AvailableStock);
        Assert.True(result.OnReorder);
    }

    [Fact]
    public async Task Adding_stock_is_capped_at_the_maximum_threshold()
    {
        var created = await CreateAsync(Command() with { AvailableStock = 8, RestockThreshold = 2, MaxStockThreshold = 10 });

        var response = await _client.PostAsJsonAsync($"{Products}/{created.Id}/stock/add", new { quantity = 999 });
        var result = await response.Content.ReadFromJsonAsync<StockChangedResponse>();

        Assert.Equal(2, result!.QuantityChanged);
        Assert.Equal(10, result.AvailableStock);
    }

    [Fact]
    public async Task Update_replaces_the_descriptive_fields()
    {
        var created = await CreateAsync(Command());

        var response = await _client.PutAsJsonAsync($"{Products}/{created.Id}", new
        {
            name = "Trackball",
            description = "Ergonomic trackball",
            pictureFileName = "ball.png",
            pictureUri = "https://images.example/ball.png"
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var fetched = await _client.GetFromJsonAsync<CatalogItemDto>($"{Products}/{created.Id}");
        Assert.Equal("Trackball", fetched!.Name);
        Assert.Equal("https://images.example/ball.png", fetched.PictureUri);
    }

    [Fact]
    public async Task Delete_removes_the_product()
    {
        var created = await CreateAsync(Command());

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"{Products}/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"{Products}/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Missing_product_returns_404()
    {
        var response = await _client.GetAsync($"{Products}/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Validation_failures_are_returned_together()
    {
        var response = await _client.PostAsJsonAsync(Products, Command() with
        {
            Name = "",
            PictureUri = "not-a-uri",
            Currency = "US"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Name", problem.Errors.Keys);
        Assert.Contains("PictureUri", problem.Errors.Keys);
        Assert.Contains("Currency", problem.Errors.Keys);
    }

    [Fact]
    public async Task Unknown_brand_returns_404_rather_than_a_foreign_key_error()
    {
        var response = await _client.PostAsJsonAsync(Products, Command() with { CatalogBrandId = int.MaxValue });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Broken_domain_rule_returns_400_from_the_module_handler()
    {
        var created = await CreateAsync(Command() with { AvailableStock = 0, RestockThreshold = 0, MaxStockThreshold = 10 });

        var response = await _client.PostAsJsonAsync($"{Products}/{created.Id}/stock/remove", new { quantity = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Catalog rule violated", problem!.Title);
    }

    [Fact]
    public async Task Changing_price_to_another_currency_is_rejected()
    {
        var created = await CreateAsync(Command() with { Currency = "USD" });

        var response = await _client.PutAsJsonAsync($"{Products}/{created.Id}/price", new { price = 10m, currency = "EUR" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Lookup_endpoints_expose_brands_and_types()
    {
        var brands = await _client.GetAsync("/api/catalog/brands");
        var types = await _client.GetAsync("/api/catalog/types");

        Assert.Equal(HttpStatusCode.OK, brands.StatusCode);
        Assert.Equal(HttpStatusCode.OK, types.StatusCode);
    }

    private async Task<CatalogItemDto> CreateAsync(CreateProductCommand command)
    {
        var response = await _client.PostAsJsonAsync(Products, command);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CatalogItemDto>())!;
    }

    private CreateProductCommand Command() => new(
        Name: $"Product-{Guid.NewGuid():N}"[..20],
        Description: "An integration test product",
        Price: 99.99m,
        PictureFileName: "p.png",
        PictureUri: "https://images.example/p.png",
        CatalogTypeId: _typeId,
        CatalogBrandId: _brandId,
        AvailableStock: 10,
        RestockThreshold: 2,
        MaxStockThreshold: 20);
}
