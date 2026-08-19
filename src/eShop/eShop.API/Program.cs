using Catalog.API.Extensions;
using Catalog.Application.Extensions;
using Catalog.Infrastructure.Extensions;
using eShop.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Module handlers run first and decline anything that is not theirs; the global
// handler covers the cross-module technical failures.
builder.Services.AddExceptionHandler<CatalogExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Each module contributes its own controllers, handlers and persistence.
// The host knows the module registration entry points and nothing else.
builder.Services
    .AddControllers()
    .AddCatalogApi();

builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(
    builder.Configuration.GetConnectionString("Catalog")
        ?? throw new InvalidOperationException("Connection string 'Catalog' is not configured."));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Convenience for local runs only. Production applies a migration bundle as
    // a deployment step rather than migrating from inside the application.
    if (app.Configuration.GetValue("Catalog:MigrateOnStartup", true))
    {
        await app.Services.InitialiseCatalogAsync(seed: true);
    }
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Exposed so integration tests can drive the real host through WebApplicationFactory.
public partial class Program;
