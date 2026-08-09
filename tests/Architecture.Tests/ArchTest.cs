namespace Architecture.Tests;

using System.Reflection;
using NetArchTest.Rules;

public class ArchitectureTest
{
    [Fact]
    public void Modules_Should_Not_Reference_Each_Other_Directly()
    {
        var moduleNames = new[] {"Catalog", "Products", "Ordering", "Payments"};
        var components = new[] {"API", "Domain", "Infrastructure", "Application"};

        foreach (var module in moduleNames) {
            foreach (var component in components) {
                var others = moduleNames.Where(m => m != module);
                var assembly = Assembly.Load($"{module}.{component}");

                var result = Types.InAssembly(assembly)
                    .That()
                    .ResideInNamespace(module)
                    .Should()
                    .NotHaveDependencyOnAny(others.ToArray())
                    .GetResult();

                Assert.True(result.IsSuccessful, $"{module} has a forbidden dependency on: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
            }
        }
    }
}
