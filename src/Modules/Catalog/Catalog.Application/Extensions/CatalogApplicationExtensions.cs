using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace Catalog.Application.Extensions;

public static class CatalogApplicationExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(CatalogApplicationExtensions).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(CatalogApplicationExtensions).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Va))

        return services;
    }
}
