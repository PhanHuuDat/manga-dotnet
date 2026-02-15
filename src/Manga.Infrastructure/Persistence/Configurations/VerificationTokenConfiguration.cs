using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class VerificationTokenConfiguration : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.ToTable("verification_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(t => t.TokenType)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.Token);

        builder.HasOne(t => t.User)
            .WithMany(u => u.VerificationTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsUsed);
        builder.Ignore(t => t.IsValid);
    }
}
