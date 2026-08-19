using Catalog.Domain;

namespace Catalog.Application.Abstractions;

// <summary> Write-side access to the CatalogItem aggregate. Returns tracked
// aggregates so domain behaviour can be invoked and persisted. </summary>

public interface ICatalogItemRepository
{
    Task<CatalogItem?> GetByIdAsync(int id, CancellationToken cancellationToken);

    void Add(CatalogItem item);

    void Remove(CatalogItem item);
}
