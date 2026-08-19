using Catalog.Application.Abstractions;
using Catalog.Domain;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public sealed class CatalogItemRepository(CatalogContext context) : ICatalogItemRepository
{
    // Tracked on purpose: the caller invokes domain behaviour on what comes
    // back, and the change tracker is what turns that into an UPDATE.
    public Task<CatalogItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.CatalogItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public void Add(CatalogItem item) => context.CatalogItems.Add(item);

    public void Remove(CatalogItem item) => context.CatalogItems.Remove(item);
}
