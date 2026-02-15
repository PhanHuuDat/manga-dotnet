using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("genres");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.Slug)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.Description)
            .HasMaxLength(200);

        builder.HasIndex(g => g.Name).IsUnique();
        builder.HasIndex(g => g.Slug).IsUnique();

        // Seed data matching frontend genres (deterministic GUIDs for idempotent migrations)
        var ns = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        builder.HasData(
            new { Id = DeterministicGuid(ns, "action"), Name = "Action", Slug = "action", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "adventure"), Name = "Adventure", Slug = "adventure", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "comedy"), Name = "Comedy", Slug = "comedy", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "drama"), Name = "Drama", Slug = "drama", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "fantasy"), Name = "Fantasy", Slug = "fantasy", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "horror"), Name = "Horror", Slug = "horror", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "mystery"), Name = "Mystery", Slug = "mystery", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "romance"), Name = "Romance", Slug = "romance", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "sci-fi"), Name = "Sci-Fi", Slug = "sci-fi", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "slice-of-life"), Name = "Slice of Life", Slug = "slice-of-life", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "sports"), Name = "Sports", Slug = "sports", Description = (string?)null },
            new { Id = DeterministicGuid(ns, "supernatural"), Name = "Supernatural", Slug = "supernatural", Description = (string?)null }
        );
    }

    /// <summary>
    /// Generate a deterministic GUID from namespace + name using UUID v5 (SHA-1).
    /// </summary>
    private static Guid DeterministicGuid(Guid namespaceId, string name)
    {
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);

        byte[] hash;
        var combined = new byte[namespaceBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, combined, 0, namespaceBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, combined, namespaceBytes.Length, nameBytes.Length);
        hash = System.Security.Cryptography.SHA1.HashData(combined);

        var result = new byte[16];
        Array.Copy(hash, 0, result, 0, 16);
        result[6] = (byte)((result[6] & 0x0F) | 0x50); // version 5
        result[8] = (byte)((result[8] & 0x3F) | 0x80); // variant RFC 4122

        SwapByteOrder(result);
        return new Guid(result);
    }

    private static void SwapByteOrder(byte[] guid)
    {
        (guid[0], guid[3]) = (guid[3], guid[0]);
        (guid[1], guid[2]) = (guid[2], guid[1]);
        (guid[4], guid[5]) = (guid[5], guid[4]);
        (guid[6], guid[7]) = (guid[7], guid[6]);
    }
}
