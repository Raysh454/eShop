namespace Catalog.Tests.Integration.Fixtures;

// <summary> One container shared across every test class in the collection.
// Starting SQL Server per class would dominate the run time. </summary>

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<CatalogDatabaseFixture>
{
    public const string Name = "catalog-database";
}
