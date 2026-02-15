using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class ReadingHistoryConfiguration : IEntityTypeConfiguration<ReadingHistory>
{
    public void Configure(EntityTypeBuilder<ReadingHistory> builder)
    {
        builder.ToTable("reading_histories");

        builder.HasKey(rh => rh.Id);

        builder.Property(rh => rh.LastPageNumber)
            .HasDefaultValue(1);

        builder.Property(rh => rh.LastReadAt)
            .IsRequired();

        builder.HasOne(rh => rh.User)
            .WithMany(u => u.ReadingHistories)
            .HasForeignKey(rh => rh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rh => rh.MangaSeries)
            .WithMany(m => m.ReadingHistories)
            .HasForeignKey(rh => rh.MangaSeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rh => rh.Chapter)
            .WithMany(ch => ch.ReadingHistories)
            .HasForeignKey(rh => rh.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rh => new { rh.UserId, rh.MangaSeriesId }).IsUnique();
    }
}
