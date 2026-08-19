using BuildingBlocks.Application.CQRS;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.ChangeProductPrice;

public record ChangeProductPriceCommand(int Id, decimal Price, string Currency = Money.DefaultCurrency) : ICommand;
