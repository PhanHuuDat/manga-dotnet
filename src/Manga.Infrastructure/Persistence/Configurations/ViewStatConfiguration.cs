using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class ViewStatConfiguration : IEntityTypeConfiguration<ViewStat>
{
    public void Configure(EntityTypeBuilder<ViewStat> builder)
    {
        builder.ToTable("view_stats");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.TargetType)
            .IsRequired();

        builder.Property(v => v.TargetId)
            .IsRequired();

        builder.Property(v => v.ViewDate)
            .IsRequired();

        builder.Property(v => v.ViewCount)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(v => v.UniqueViewCount)
            .IsRequired()
            .HasDefaultValue(0L);

        // Composite unique index — ensures 1 row per target per day, enables upsert via ON CONFLICT
        builder.HasIndex(v => new { v.TargetType, v.TargetId, v.ViewDate })
            .IsUnique();

        // Trending queries: filter by type + date range
        builder.HasIndex(v => new { v.TargetType, v.ViewDate });

        // Per-entity lookup
        builder.HasIndex(v => v.TargetId);
    }
}
