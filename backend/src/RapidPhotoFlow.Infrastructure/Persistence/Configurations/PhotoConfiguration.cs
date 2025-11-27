using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Photo entity.
/// </summary>
public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("Photos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new PhotoId(value))
            .ValueGeneratedNever();

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(1000);

        builder.Ignore(p => p.DomainEvents);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.UploadedAt);
    }
}

