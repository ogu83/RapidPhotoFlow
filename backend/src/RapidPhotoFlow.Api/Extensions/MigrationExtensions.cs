using Microsoft.EntityFrameworkCore;
using RapidPhotoFlow.Infrastructure.Persistence;

namespace RapidPhotoFlow.Api.Extensions;

/// <summary>
/// Extension methods for database migration.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending database migrations on application startup.
    /// </summary>
    public static async Task UseDatabaseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}

