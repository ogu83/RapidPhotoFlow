using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RapidPhotoFlow.Domain.EventLog;

namespace RapidPhotoFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for EventLogEntry entity.
/// </summary>
public class EventLogEntryConfiguration : IEntityTypeConfiguration<EventLogEntry>
{
    public void Configure(EntityTypeBuilder<EventLogEntry> builder)
    {
        builder.ToTable("EventLogEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.MetadataJson)
            .HasMaxLength(4000);

        builder.HasIndex(e => e.PhotoId);
        builder.HasIndex(e => e.CreatedAt);
    }
}

