using FluentValidation;

namespace Catalog.Application.Features.Products.DeleteProduct;

public sealed class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
    }
}
