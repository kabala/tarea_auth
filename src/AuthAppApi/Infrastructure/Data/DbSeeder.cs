using AuthApp.Api.Application.Abstractions;
using AuthApp.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Api.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var users = new[]
        {
            new User
            {
                Email = "demo@example.com",
                PasswordHash = hasher.Hash("Password123!"),
                FullName = "Usuario Demo",
                Role = "user",
                CreatedAt = now,
                UpdatedAt = now,
            },
            new User
            {
                Email = "admin@example.com",
                PasswordHash = hasher.Hash("Admin123!"),
                FullName = "Administrador",
                Role = "admin",
                CreatedAt = now,
                UpdatedAt = now,
            },
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();
    }
}
