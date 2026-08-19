using Catalog.Application.Abstractions;
using Catalog.Infrastructure.Data;

namespace Catalog.Infrastructure.Repositories;

public sealed class UnitOfWork(CatalogContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
