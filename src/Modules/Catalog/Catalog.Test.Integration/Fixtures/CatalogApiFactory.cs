using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Catalog.Tests.Integration.Fixtures;

// <summary> Boots the real host against the test container. Migration on
// startup is disabled because the fixture has already applied migrations, and
// seeding would otherwise add rows the assertions do not expect. </summary>

public sealed class CatalogApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Catalog", connectionString);
        builder.UseSetting("Catalog:MigrateOnStartup", "false");
    }
}
