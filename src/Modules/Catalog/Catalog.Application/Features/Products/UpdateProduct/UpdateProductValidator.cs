using Catalog.Domain;
using FluentValidation;

namespace Catalog.Application.Features.Products.UpdateProduct;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(CatalogItem.MaxNameLength);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(CatalogItem.MaxDescriptionLength);
        RuleFor(c => c.PictureFileName).NotEmpty().MaximumLength(CatalogItem.MaxPictureFileNameLength);
        RuleFor(c => c.PictureUri)
            .NotEmpty()
            .MaximumLength(CatalogItem.MaxPictureUriLength)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("PictureUri must be an absolute URI.");
    }
}
