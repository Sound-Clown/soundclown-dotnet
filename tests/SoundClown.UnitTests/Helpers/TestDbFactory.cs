using Microsoft.EntityFrameworkCore;
using MusicApp.Data;
using MusicApp.Entities;
using MusicApp.Enums;

namespace SoundClown.UnitTests.Helpers;

/// <summary>
/// Creates isolated in-memory AppDbContext per test, pre-seeded with a small fixture
/// (3 users: listener/artistA/artistB/admin, 2 albums, 3 songs).
/// </summary>
public static class TestDbFactory
{
    public const int ListenerId = 1;
    public const int ArtistAId = 2;
    public const int ArtistBId = 3;
    public const int AdminId = 4;
    public const int ApprovedSongId = 10;   // by ArtistA, status=Approved, like_count=5
    public const int PendingSongId = 11;    // by ArtistA, status=Pending
    public const int ArtistBSongId = 12;    // by ArtistB, status=Approved
    public const int ArtistAAlbumId = 100;
    public const int ArtistBAlbumId = 101;

    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"sc-test-{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);
        Seed(db);
        return db;
    }

    private static void Seed(AppDbContext db)
    {
        db.Users.AddRange(
            new User { Id = ListenerId, Username = "listener", Email = "listener@test.com", PasswordHash = "x", Role = Role.Listener },
            new User { Id = ArtistAId, Username = "artistA", Email = "artistA@test.com", PasswordHash = "x", Role = Role.Artist },
            new User { Id = ArtistBId, Username = "artistB", Email = "artistB@test.com", PasswordHash = "x", Role = Role.Artist },
            new User { Id = AdminId, Username = "admin", Email = "admin@test.com", PasswordHash = "x", Role = Role.Admin }
        );
        db.Albums.AddRange(
            new Album { Id = ArtistAAlbumId, Name = "Album A1", ArtistId = ArtistAId },
            new Album { Id = ArtistBAlbumId, Name = "Album B1", ArtistId = ArtistBId }
        );
        db.Songs.AddRange(
            new Song { Id = ApprovedSongId, Title = "Approved Song", AudioFile = "a.mp3", ArtistId = ArtistAId, Status = SongStatus.Approved, LikeCount = 5, PlayCount = 100 },
            new Song { Id = PendingSongId, Title = "Pending Song", AudioFile = "b.mp3", ArtistId = ArtistAId, Status = SongStatus.Pending },
            new Song { Id = ArtistBSongId, Title = "Artist B Track", AudioFile = "c.mp3", ArtistId = ArtistBId, Status = SongStatus.Approved }
        );
        db.SaveChanges();
    }
}
