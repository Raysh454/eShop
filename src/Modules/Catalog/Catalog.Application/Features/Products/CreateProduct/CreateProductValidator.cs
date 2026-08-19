using Catalog.Domain;
using FluentValidation;

namespace Catalog.Application.Features.Products.CreateProduct;

// <summary> Rejects malformed input at the edge so the aggregate is only ever
// constructed with plausible values. The aggregate still enforces the same
// rules; this exists to return a 400 with all failures at once rather than
// surfacing the first broken invariant. </summary>

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(CatalogItem.MaxNameLength);

        RuleFor(c => c.Description)
            .NotEmpty()
            .MaximumLength(CatalogItem.MaxDescriptionLength);

        RuleFor(c => c.PictureFileName)
            .NotEmpty()
            .MaximumLength(CatalogItem.MaxPictureFileNameLength);

        RuleFor(c => c.PictureUri)
            .NotEmpty()
            .MaximumLength(CatalogItem.MaxPictureUriLength)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("'{PropertyName}' must be an absolute URI.");

        RuleFor(c => c.Price)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);

        RuleFor(c => c.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("'{PropertyName}' must be a three-letter ISO 4217 code.");

        RuleFor(c => c.CatalogTypeId).GreaterThan(0);
        RuleFor(c => c.CatalogBrandId).GreaterThan(0);

        RuleFor(c => c.MaxStockThreshold).GreaterThan(0);
        RuleFor(c => c.RestockThreshold)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(c => c.MaxStockThreshold)
            .WithMessage("'{PropertyName}' cannot exceed MaxStockThreshold.");
        RuleFor(c => c.AvailableStock)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(c => c.MaxStockThreshold)
            .WithMessage("'{PropertyName}' cannot exceed MaxStockThreshold.");
    }
}
