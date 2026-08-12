namespace Catalog.Tests.Integration;

public class CatalogInfrastructureTests
{
    [Fact(Skip = "Enabled when the Catalog SQL Server and RabbitMQ Testcontainers fixture is introduced.")]
    public void Catalog_persistence_and_messaging_are_verified_through_integration_tests()
    {
    }
}
