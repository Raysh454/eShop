using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Tests.Unit.Domain;

public class MoneyTests
{
    [Fact]
    public void From_KeepsAmountAndCurrency()
    {
        var money = Money.From(12.34m, "EUR");

        Assert.Equal(12.34m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void From_DefaultsToTheDefaultCurrency()
    {
        Assert.Equal(Money.DefaultCurrency, Money.From(1m).Currency);
    }

    [Theory]
    [InlineData("usd")]
    [InlineData(" usd ")]
    [InlineData("Usd")]
    public void From_NormalisesCurrencyCasingAndWhitespace(string currency)
    {
        Assert.Equal("USD", Money.From(1m, currency).Currency);
    }

    [Fact]
    public void Zero_IsZeroInTheGivenCurrency()
    {
        var zero = Money.Zero("GBP");

        Assert.Equal(0m, zero.Amount);
        Assert.Equal("GBP", zero.Currency);
    }

    [Fact]
    public void From_WithNegativeAmount_Throws()
    {
        Assert.Throws<CatalogDomainException>(() => Money.From(-0.01m));
    }

    [Fact]
    public void From_WithMoreThanTwoDecimalPlaces_Throws()
    {
        Assert.Throws<CatalogDomainException>(() => Money.From(1.005m));
    }

    [Fact]
    public void From_WithTrailingZeroesBeyondTwoPlaces_IsAccepted()
    {
        // 1.9900 loses nothing when stored as decimal(18,2); only real precision
        // beyond two places is rejected.
        Assert.Equal(1.99m, Money.From(1.9900m).Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("12A")]
    [InlineData("US$")]
    public void From_WithInvalidCurrency_Throws(string currency)
    {
        Assert.Throws<CatalogDomainException>(() => Money.From(1m, currency));
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.Equal(Money.From(9.99m, "USD"), Money.From(9.99m, "USD"));
        Assert.Equal(Money.From(9.99m, "USD"), Money.From(9.99m, "usd"));
    }

    [Fact]
    public void Equality_DistinguishesCurrency()
    {
        Assert.NotEqual(Money.From(9.99m, "USD"), Money.From(9.99m, "EUR"));
    }

    [Fact]
    public void Equality_IgnoresTrailingZeroScale()
    {
        Assert.Equal(Money.From(10m), Money.From(10.00m));
    }

    [Fact]
    public void Add_SumsAmountsInTheSameCurrency()
    {
        Assert.Equal(Money.From(3.75m), Money.From(1.25m) + Money.From(2.50m));
    }

    [Fact]
    public void Subtract_ReducesTheAmount()
    {
        Assert.Equal(Money.From(1.25m), Money.From(3.75m) - Money.From(2.50m));
    }

    [Fact]
    public void Subtract_BelowZero_Throws()
    {
        Assert.Throws<CatalogDomainException>(() => Money.From(1m) - Money.From(2m));
    }

    [Fact]
    public void Add_AcrossCurrencies_Throws()
    {
        Assert.Throws<CatalogDomainException>(() => Money.From(1m, "USD") + Money.From(1m, "EUR"));
    }

    [Fact]
    public void ToString_RendersTwoDecimalPlacesAndCurrency()
    {
        Assert.Equal("9.50 USD", Money.From(9.5m).ToString());
    }
}
