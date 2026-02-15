using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class AlternativeTitleConfiguration : IEntityTypeConfiguration<AlternativeTitle>
{
    public void Configure(EntityTypeBuilder<AlternativeTitle> builder)
    {
        builder.ToTable("alternative_titles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasOne(a => a.MangaSeries)
            .WithMany(m => m.AlternativeTitles)
            .HasForeignKey(a => a.MangaSeriesId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
