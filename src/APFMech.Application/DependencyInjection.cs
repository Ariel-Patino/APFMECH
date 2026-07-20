using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace APFMech.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR handlers and pipelines from Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register FluentValidation validators automatically
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}