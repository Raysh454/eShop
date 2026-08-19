using Catalog.Application.Abstractions;
using Catalog.Domain;

namespace Catalog.Tests.Unit.Application;

// <summary> In-memory stand-in for the write-side port. Records whether the
// unit of work was committed, so a handler that mutates an aggregate but never
// saves is caught. </summary>

internal sealed class FakeCatalogItemRepository : ICatalogItemRepository, IUnitOfWork
{
    private readonly Dictionary<int, CatalogItem> _items = [];
    private int _nextId = 1;

    public int SaveCount { get; private set; }

    public List<CatalogItem> Added { get; } = [];

    public List<CatalogItem> Removed { get; } = [];

    public CatalogItem Seed(CatalogItem item)
    {
        var id = _nextId++;
        SetId(item, id);
        _items[id] = item;
        return item;
    }

    public Task<CatalogItem?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_items.GetValueOrDefault(id));

    public void Add(CatalogItem item)
    {
        // Mirrors HiLo, which assigns the identity when the item is tracked
        // rather than when the transaction commits.
        SetId(item, _nextId++);
        _items[item.Id] = item;
        Added.Add(item);
    }

    public void Remove(CatalogItem item)
    {
        _items.Remove(item.Id);
        Removed.Add(item);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    // Id has a protected setter, which is correct for production code: only the
    // persistence layer assigns it. Reflection is the least invasive way to
    // emulate that here without widening the aggregate's surface for tests.
    private static void SetId(CatalogItem item, int id) =>
        typeof(CatalogItem)
            .GetProperty(nameof(CatalogItem.Id))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(item, [id]);
}
