using FluentValidation;

namespace Catalog.Application.Features.Products.ChangeProductPrice;

public sealed class ChangeProductPriceValidator : AbstractValidator<ChangeProductPriceCommand>
{
    public ChangeProductPriceValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
        RuleFor(c => c.Price).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(c => c.Currency)
            .NotEmpty()
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("Currency must be a three-letter ISO 4217 code.");
    }
}
