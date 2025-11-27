using Microsoft.Extensions.DependencyInjection;

namespace RapidPhotoFlow.Application.DependencyInjection;

/// <summary>
/// Extension methods for adding Application layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        });

        return services;
    }
}

