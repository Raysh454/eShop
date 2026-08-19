using FluentValidation;

namespace Catalog.Application.Features.Products.RemoveStock;

public sealed class RemoveStockValidator : AbstractValidator<RemoveStockCommand>
{
    public RemoveStockValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}
