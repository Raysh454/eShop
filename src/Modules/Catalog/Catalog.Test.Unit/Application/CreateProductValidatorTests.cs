using Catalog.Application.Features.Products.CreateProduct;
using Catalog.Domain;

namespace Catalog.Tests.Unit.Application;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankName_Fails(string name) =>
        AssertInvalid(Valid() with { Name = name }, nameof(CreateProductCommand.Name));

    [Fact]
    public void OverlongName_Fails() =>
        AssertInvalid(Valid() with { Name = new string('a', CatalogItem.MaxNameLength + 1) }, nameof(CreateProductCommand.Name));

    [Fact]
    public void BlankDescription_Fails() =>
        AssertInvalid(Valid() with { Description = "" }, nameof(CreateProductCommand.Description));

    [Fact]
    public void BlankPictureFileName_Fails() =>
        AssertInvalid(Valid() with { PictureFileName = "" }, nameof(CreateProductCommand.PictureFileName));

    [Theory]
    [InlineData("")]
    [InlineData("not-a-uri")]
    [InlineData("/relative/path.png")]
    public void NonAbsolutePictureUri_Fails(string uri) =>
        AssertInvalid(Valid() with { PictureUri = uri }, nameof(CreateProductCommand.PictureUri));

    [Fact]
    public void NegativePrice_Fails() =>
        AssertInvalid(Valid() with { Price = -0.01m }, nameof(CreateProductCommand.Price));

    [Fact]
    public void PriceWithMoreThanTwoDecimalPlaces_Fails() =>
        AssertInvalid(Valid() with { Price = 1.005m }, nameof(CreateProductCommand.Price));

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("12A")]
    public void InvalidCurrency_Fails(string currency) =>
        AssertInvalid(Valid() with { Currency = currency }, nameof(CreateProductCommand.Currency));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveTypeId_Fails(int typeId) =>
        AssertInvalid(Valid() with { CatalogTypeId = typeId }, nameof(CreateProductCommand.CatalogTypeId));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveBrandId_Fails(int brandId) =>
        AssertInvalid(Valid() with { CatalogBrandId = brandId }, nameof(CreateProductCommand.CatalogBrandId));

    [Fact]
    public void NonPositiveMaxStockThreshold_Fails() =>
        AssertInvalid(Valid() with { MaxStockThreshold = 0 }, nameof(CreateProductCommand.MaxStockThreshold));

    [Fact]
    public void RestockThresholdAboveMaximum_Fails() =>
        AssertInvalid(Valid() with { RestockThreshold = 21, MaxStockThreshold = 20 }, nameof(CreateProductCommand.RestockThreshold));

    [Fact]
    public void AvailableStockAboveMaximum_Fails() =>
        AssertInvalid(Valid() with { AvailableStock = 21, MaxStockThreshold = 20 }, nameof(CreateProductCommand.AvailableStock));

    [Fact]
    public void NegativeAvailableStock_Fails() =>
        AssertInvalid(Valid() with { AvailableStock = -1 }, nameof(CreateProductCommand.AvailableStock));

    [Fact]
    public void AllFailuresAreReportedTogether()
    {
        // The point of validating at the edge: the caller sees every problem at
        // once instead of the first broken invariant the aggregate happens to hit.
        var result = _validator.Validate(Valid() with { Name = "", Description = "", CatalogBrandId = 0 });

        Assert.Equal(3, result.Errors.Count);
    }

    private void AssertInvalid(CreateProductCommand command, string propertyName)
    {
        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == propertyName);
    }

    private static CreateProductCommand Valid() => new(
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
