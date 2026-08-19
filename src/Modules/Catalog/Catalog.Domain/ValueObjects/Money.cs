using Catalog.Domain.Exceptions;

namespace Catalog.Domain.ValueObjects;

// <summary> A non-negative monetary amount in a single currency. Scale is capped
// at two decimal places to match the decimal(18,2) column and to make silent
// rounding on save impossible. </summary>

public sealed record Money
{
    public const string DefaultCurrency = "USD";

    private const int Scale = 2;

    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money From(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new CatalogDomainException("Amount cannot be negative.");

        if (decimal.Round(amount, Scale) != amount)
            throw new CatalogDomainException($"Amount cannot have more than {Scale} decimal places.");

        return new Money(amount, NormalizeCurrency(currency));
    }

    public static Money Zero(string currency = DefaultCurrency) => From(0m, currency);

    public Money Add(Money other) => From(Amount + EnsureSameCurrency(other).Amount, Currency);

    public Money Subtract(Money other) => From(Amount - EnsureSameCurrency(other).Amount, Currency);

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public override string ToString() => $"{Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} {Currency}";

    private Money EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
            throw new CatalogDomainException($"Cannot combine {Currency} with {other.Currency}.");

        return other;
    }

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new CatalogDomainException("Currency is required.");

        var normalized = currency.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || !normalized.All(char.IsAsciiLetterUpper))
            throw new CatalogDomainException("Currency must be a three-letter ISO 4217 code.");

        return normalized;
    }
}
