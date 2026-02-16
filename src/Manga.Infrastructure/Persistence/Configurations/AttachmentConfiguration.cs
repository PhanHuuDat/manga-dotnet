using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.Url)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.FileSize)
            .IsRequired();

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(a => a.ThumbnailStoragePath)
            .HasMaxLength(500);

        builder.HasIndex(a => a.Type);
    }
}
