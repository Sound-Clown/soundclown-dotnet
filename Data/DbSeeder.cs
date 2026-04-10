using Microsoft.EntityFrameworkCore;
using MusicApp.Entities;
using MusicApp.Enums;

namespace MusicApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var admin = new User
        {
            Username = "admin",
            Email = "admin@music.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123456!"),
            Role = Role.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var listener = new User
        {
            Username = "listener",
            Email = "listener@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Listener123!"),
            Role = Role.Listener,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var artist = new User
        {
            Username = "artist",
            Email = "artist@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Artist123!"),
            Role = Role.Artist,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(admin, listener, artist);
        await db.SaveChangesAsync();
    }
}