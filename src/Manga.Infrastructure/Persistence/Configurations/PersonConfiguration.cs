using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manga.Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.PersonNumber)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(p => p.Biography)
            .HasColumnType("text");

        builder.HasOne(p => p.Photo)
            .WithMany()
            .HasForeignKey(p => p.PhotoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.PersonNumber).IsUnique();
        builder.HasIndex(p => p.Name);
    }
}
