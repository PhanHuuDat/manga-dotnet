using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.ToTable("chapters");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ChapterNumber)
            .HasPrecision(6, 1)
            .IsRequired();

        builder.Property(c => c.Title)
            .HasMaxLength(300);

        builder.Property(c => c.Slug)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(c => c.Pages)
            .HasDefaultValue(0);

        builder.Property(c => c.Views)
            .HasDefaultValue(0L);

        builder.Property(c => c.PublishedAt)
            .IsRequired();

        builder.HasOne(c => c.MangaSeries)
            .WithMany(m => m.Chapters)
            .HasForeignKey(c => c.MangaSeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.MangaSeriesId, c.ChapterNumber }).IsUnique();
        builder.HasIndex(c => c.Slug);
        builder.HasIndex(c => c.PublishedAt).IsDescending();
    }
}
