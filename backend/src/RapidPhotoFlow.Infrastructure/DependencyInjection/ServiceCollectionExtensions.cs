using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Abstractions.Processing;
using RapidPhotoFlow.Application.Abstractions.Storage;
using RapidPhotoFlow.Infrastructure.Persistence;
using RapidPhotoFlow.Infrastructure.Persistence.Repositories;
using RapidPhotoFlow.Infrastructure.Processing;
using RapidPhotoFlow.Infrastructure.Storage;

namespace RapidPhotoFlow.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for adding Infrastructure layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string uploadPath)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IEventLogRepository, EventLogRepository>();

        // Storage
        services.AddSingleton<IPhotoStorage>(new LocalFilePhotoStorage(uploadPath));

        // Processing Queue
        services.AddSingleton<IPhotoProcessingQueue, InMemoryPhotoProcessingQueue>();
        services.AddHostedService<PhotoProcessingWorker>();

        return services;
    }
}

