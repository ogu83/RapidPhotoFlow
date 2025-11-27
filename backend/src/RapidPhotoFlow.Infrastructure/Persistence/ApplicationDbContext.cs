using Microsoft.EntityFrameworkCore;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Domain.EventLog;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Infrastructure.Persistence;

/// <summary>
/// Application database context.
/// </summary>
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<EventLogEntry> EventLogEntries => Set<EventLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

