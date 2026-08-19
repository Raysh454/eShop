namespace Catalog.Application.Abstractions;

// <summary> Read-side query parameters. A separate type from the query slice so
// the port does not depend on any one feature folder. </summary>

public sealed record ProductFilter(int Page, int PageSize, int? BrandId, int? TypeId, string? Search);
