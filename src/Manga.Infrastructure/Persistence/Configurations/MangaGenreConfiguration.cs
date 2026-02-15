using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class MangaGenreConfiguration : IEntityTypeConfiguration<MangaGenre>
{
    public void Configure(EntityTypeBuilder<MangaGenre> builder)
    {
        builder.ToTable("manga_genres");

        builder.HasKey(mg => new { mg.MangaSeriesId, mg.GenreId });

        builder.HasOne(mg => mg.MangaSeries)
            .WithMany(m => m.MangaGenres)
            .HasForeignKey(mg => mg.MangaSeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mg => mg.Genre)
            .WithMany(g => g.MangaGenres)
            .HasForeignKey(mg => mg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
