using System.Reflection;
using NetArchTest.Rules;

namespace Architecture.Tests;

public class ArchitectureTest
{
    private static readonly string[] OtherModuleNamespaces = ["Basket", "Ordering", "Payments"];

    [Fact]
    public void CatalogAssemblies_Should_Not_Reference_OtherModules()
    {
        var catalogAssemblies = new[]
        {
            "Catalog.API",
            "Catalog.Application",
            "Catalog.Contracts",
            "Catalog.Domain",
            "Catalog.Infrastructure"
        };

        foreach (var assemblyName in catalogAssemblies)
        {
            var result = Types.InAssembly(Assembly.Load(assemblyName))
                .Should()
                .NotHaveDependencyOnAny(OtherModuleNamespaces)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{assemblyName} has a forbidden dependency: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    [Fact]
    public void CatalogContracts_Should_Not_Depend_On_CatalogImplementation()
    {
        var result = Types.InAssembly(Assembly.Load("Catalog.Contracts"))
            .Should()
            .NotHaveDependencyOnAny(["Catalog.Domain", "Catalog.Application", "Catalog.Infrastructure", "Catalog.API"])
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Catalog.Contracts must remain a standalone integration-event boundary.");
    }
}
