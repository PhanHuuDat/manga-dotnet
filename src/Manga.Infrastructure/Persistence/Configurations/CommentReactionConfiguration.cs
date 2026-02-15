using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
{
    public void Configure(EntityTypeBuilder<CommentReaction> builder)
    {
        builder.ToTable("comment_reactions");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.ReactionType)
            .IsRequired();

        builder.HasOne(cr => cr.User)
            .WithMany(u => u.CommentReactions)
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.Comment)
            .WithMany(c => c.Reactions)
            .HasForeignKey(cr => cr.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cr => new { cr.UserId, cr.CommentId }).IsUnique();
    }
}
