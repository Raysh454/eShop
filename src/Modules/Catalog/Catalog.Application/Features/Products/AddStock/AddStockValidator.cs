using FluentValidation;

namespace Catalog.Application.Features.Products.AddStock;

public sealed class AddStockValidator : AbstractValidator<AddStockCommand>
{
    public AddStockValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}
