using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MusicApp.Entities;

namespace MusicApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasMany(e => e.Songs)
                .WithOne(s => s.Artist)
                .HasForeignKey(s => s.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Albums)
                .WithOne(a => a.Artist)
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Song ──────────────────────────────────────────────
        modelBuilder.Entity<Song>(entity =>
        {
            entity.ToTable("songs");
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ArtistId);

            entity.HasOne(e => e.Album)
                .WithMany(a => a.Songs)
                .HasForeignKey(e => e.AlbumId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Likes)
                .WithOne(l => l.Song)
                .HasForeignKey(l => l.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Album ──────────────────────────────────────────────
        modelBuilder.Entity<Album>(entity =>
        {
            entity.ToTable("albums");
        });

        // ── Like (composite PK) ────────────────────────────────
        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("likes");
            entity.HasKey(e => new { e.UserId, e.SongId });
        });

        // ── PasswordResetToken ────────────────────────────────
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();
        });
    }
}