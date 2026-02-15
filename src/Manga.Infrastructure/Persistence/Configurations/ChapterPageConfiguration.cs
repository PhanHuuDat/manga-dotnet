using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class ChapterPageConfiguration : IEntityTypeConfiguration<ChapterPage>
{
    public void Configure(EntityTypeBuilder<ChapterPage> builder)
    {
        builder.ToTable("chapter_pages");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.PageNumber)
            .IsRequired();

        builder.Property(cp => cp.ImageId)
            .IsRequired();

        builder.HasOne(cp => cp.Chapter)
            .WithMany(c => c.ChapterPages)
            .HasForeignKey(cp => cp.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Image)
            .WithMany()
            .HasForeignKey(cp => cp.ImageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cp => new { cp.ChapterId, cp.PageNumber }).IsUnique();
    }
}
