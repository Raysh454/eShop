using FluentValidation;

namespace Catalog.Application.Features.Products.GetProducts;

public sealed class GetProductsValidator : AbstractValidator<GetProductsQuery>
{
    public const int MaxPageSize = 100;

    public GetProductsValidator()
    {
        RuleFor(q => q.Page).GreaterThan(0);

        // Capped so a caller cannot ask for the whole table in one request.
        RuleFor(q => q.PageSize).InclusiveBetween(1, MaxPageSize);

        RuleFor(q => q.BrandId).GreaterThan(0).When(q => q.BrandId.HasValue);
        RuleFor(q => q.TypeId).GreaterThan(0).When(q => q.TypeId.HasValue);
    }
}
