namespace Catalog.Application.Features.Products;

// <summary> Stock operations clamp to what is actually available or to the
// maximum threshold, so the caller is told what really happened rather than
// what it asked for. </summary>

public record StockChangedResponse(int Id, int QuantityChanged, int AvailableStock, bool OnReorder);
