using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class MangaSeriesConfiguration : IEntityTypeConfiguration<MangaSeries>
{
    public void Configure(EntityTypeBuilder<MangaSeries> builder)
    {
        builder.ToTable("manga_series");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(m => m.Slug)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(m => m.Synopsis)
            .HasColumnType("text");

        builder.Property(m => m.CoverUrl)
            .HasMaxLength(500);

        builder.Property(m => m.BannerUrl)
            .HasMaxLength(500);

        builder.Property(m => m.Author)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Artist)
            .HasMaxLength(200);

        builder.Property(m => m.Status)
            .HasDefaultValue(SeriesStatus.Ongoing);

        builder.Property(m => m.Rating)
            .HasPrecision(3, 2)
            .HasDefaultValue(0m);

        builder.Property(m => m.RatingCount)
            .HasDefaultValue(0);

        builder.Property(m => m.Views)
            .HasDefaultValue(0L);

        builder.Property(m => m.TotalChapters)
            .HasDefaultValue(0);

        builder.Property(m => m.LatestChapterNumber)
            .HasDefaultValue(0);

        builder.HasIndex(m => m.Slug).IsUnique();
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.Views).IsDescending();
        builder.HasIndex(m => m.CreatedAt).IsDescending();
    }
}
