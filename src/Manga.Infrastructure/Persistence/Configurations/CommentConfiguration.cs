using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(c => c.Likes)
            .HasDefaultValue(0);

        builder.Property(c => c.Dislikes)
            .HasDefaultValue(0);

        builder.Property(c => c.ReplyCount)
            .HasDefaultValue(0);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.MangaSeries)
            .WithMany(m => m.Comments)
            .HasForeignKey(c => c.MangaSeriesId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Chapter)
            .WithMany(ch => ch.Comments)
            .HasForeignKey(c => c.ChapterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.MangaSeriesId);
        builder.HasIndex(c => c.ChapterId);
        builder.HasIndex(c => c.ParentId);
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CreatedAt).IsDescending();
    }
}
