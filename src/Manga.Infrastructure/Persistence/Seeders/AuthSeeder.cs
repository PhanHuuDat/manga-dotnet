using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Manga.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds admin user and role mappings for development environment.
/// </summary>
public static class AuthSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync(u => u.Email == "admin@mangavoid.com"))
            return;

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@mangavoid.com",
            PasswordHash = passwordHasher.Hash("Admin123!"),
            DisplayName = "Admin",
            EmailConfirmed = true,
        };

        context.Users.Add(adminUser);
        context.UserRoleMappings.AddRange(
            new UserRoleMapping { UserId = adminUser.Id, Role = UserRole.Admin },
            new UserRoleMapping { UserId = adminUser.Id, Role = UserRole.Reader }
        );

        await context.SaveChangesAsync();
    }
}
