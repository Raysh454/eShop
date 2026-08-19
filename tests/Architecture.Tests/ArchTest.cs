using System.Reflection;
using NetArchTest.Rules;

namespace Architecture.Tests;

public class ArchitectureTest
{
    private static readonly string[] OtherModuleNamespaces = ["Basket", "Ordering", "Payments"];

    private static readonly string[] CatalogAssemblies =
    [
        "Catalog.API",
        "Catalog.Application",
        "Catalog.Contracts",
        "Catalog.Domain",
        "Catalog.Infrastructure"
    ];

    [Fact]
    public void CatalogAssemblies_Should_Not_Reference_OtherModules()
    {
        foreach (var assemblyName in CatalogAssemblies)
        {
            AssertNoDependency(assemblyName, OtherModuleNamespaces,
                $"{assemblyName} must not depend on another module.");
        }
    }

    [Fact]
    public void CatalogContracts_Should_Not_Depend_On_CatalogImplementation()
    {
        AssertNoDependency("Catalog.Contracts",
            ["Catalog.Domain", "Catalog.Application", "Catalog.Infrastructure", "Catalog.API"],
            "Catalog.Contracts must remain a standalone integration-event boundary.");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure_Concerns()
    {
        AssertNoDependency("Catalog.Domain",
            ["Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "RabbitMQ", "FluentValidation"],
            "Domain code must not depend on EF Core, ASP.NET Core, RabbitMQ or validation infrastructure.");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_EfCore_Or_AspNetCore()
    {
        AssertNoDependency("Catalog.Application",
            ["Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "RabbitMQ"],
            "Application code coordinates use cases; persistence and transport belong behind ports.");
    }

    [Fact]
    public void Api_Should_Not_Depend_On_EfCore()
    {
        AssertNoDependency("Catalog.API", ["Microsoft.EntityFrameworkCore"],
            "Controllers must go through MediatR, never straight to a DbContext.");
    }

    [Fact]
    public void Handlers_Should_Live_In_The_Application_Layer()
    {
        var offenders = Assembly.Load("Catalog.Infrastructure").GetTypes()
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>)))
            .Select(type => type.Name)
            .ToArray();

        Assert.True(offenders.Length == 0,
            "Request handlers belong in Application; Infrastructure may handle notifications, not requests. "
            + $"Offending types: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Commands_And_Queries_Should_Be_Sealed_Or_Records()
    {
        var offenders = Types.InAssembly(Assembly.Load("Catalog.Application"))
            .That()
            .ImplementInterface(typeof(MediatR.IRequest<>))
            .GetTypes()
            .Where(type => !type.IsSealed && !IsRecord(type))
            .Select(type => type.Name)
            .ToArray();

        Assert.True(offenders.Length == 0,
            $"Commands and queries should be records or sealed: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Every_Command_And_Query_Should_Have_A_Handler()
    {
        var applicationAssembly = Assembly.Load("Catalog.Application");

        var handledRequests = applicationAssembly.GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>))
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        var unhandled = applicationAssembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(MediatR.IRequest<>)))
            .Where(type => !handledRequests.Contains(type))
            .Select(type => type.Name)
            .ToArray();

        Assert.True(unhandled.Length == 0,
            $"These requests have no handler and would fail at runtime: {string.Join(", ", unhandled)}");
    }

    private static bool IsRecord(Type type) =>
        type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) is not null;

    private static void AssertNoDependency(string assemblyName, string[] forbidden, string because)
    {
        var result = Types.InAssembly(Assembly.Load(assemblyName))
            .Should()
            .NotHaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"{because} Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
