using Catalog.API.Extensions;
using Catalog.Application.Extensions;
using Catalog.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Exposed so integration tests can drive the real host through WebApplicationFactory.
public partial class Program;
