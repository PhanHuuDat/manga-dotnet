using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Manga.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds admin user and role mappings on startup (idempotent).
/// Credentials are sourced from environment variables: ADMIN_USERNAME, ADMIN_EMAIL, ADMIN_PASSWORD.
/// </summary>
public static class AuthSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        string username,
        string email,
        string password)
    {
        if (await context.Users.AnyAsync(u => u.Email == email))
            return;

        var adminUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.Hash(password),
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
