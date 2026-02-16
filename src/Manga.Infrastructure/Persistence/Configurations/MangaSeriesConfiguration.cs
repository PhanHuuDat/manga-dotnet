using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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

        builder.Property(m => m.SeriesNumber)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(m => m.Synopsis)
            .HasColumnType("text");

        builder.Property(m => m.AuthorId)
            .IsRequired();

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

        // FK relationships
        builder.HasOne(m => m.Cover)
            .WithMany()
            .HasForeignKey(m => m.CoverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Banner)
            .WithMany()
            .HasForeignKey(m => m.BannerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Author)
            .WithMany(p => p.AuthoredSeries)
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Artist)
            .WithMany(p => p.IllustratedSeries)
            .HasForeignKey(m => m.ArtistId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.SeriesNumber).IsUnique();
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.Views).IsDescending();
        builder.HasIndex(m => m.CreatedAt).IsDescending();
        builder.HasIndex(m => m.AuthorId);
        builder.HasIndex(m => m.ArtistId);

        // Optimistic concurrency using PostgreSQL xmin system column
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
