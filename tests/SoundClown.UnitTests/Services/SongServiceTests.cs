using FluentAssertions;
using MusicApp.DTOs;
using MusicApp.Enums;
using MusicApp.Services;
using SoundClown.UnitTests.Helpers;

namespace SoundClown.UnitTests.Services;

/// <summary>
/// Unit tests for SongService — focus on branches & rules referenced in the report
/// (TC-03 Like branch, TC-04 Unlike branch, TC-10 Idempotency, TC-16 cross-user
/// ownership, TC-17 edit resets status to Pending).
/// </summary>
public class SongServiceTests
{
    // ── TC-03 — Like branch (insert path) ───────────────────────────────────
    [Fact]
    public async Task ToggleLike_WhenNotLiked_InsertsRow_AndIncrementsCount()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ListenerId, Role.Listener));

        var result = await svc.ToggleLikeAsync(TestDbFactory.ApprovedSongId, TestDbFactory.ListenerId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Liked.Should().BeTrue();
        result.Data.NewCount.Should().Be(6); // seed=5 → 6
        db.Likes.Should().ContainSingle(l =>
            l.UserId == TestDbFactory.ListenerId && l.SongId == TestDbFactory.ApprovedSongId);
    }

    // ── TC-04 — Unlike branch (delete path) ──────────────────────────────────
    [Fact]
    public async Task ToggleLike_WhenAlreadyLiked_DeletesRow_AndDecrementsCount()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ListenerId, Role.Listener));

        // Arrange: first like
        await svc.ToggleLikeAsync(TestDbFactory.ApprovedSongId, TestDbFactory.ListenerId);

        // Act: second toggle = unlike
        var result = await svc.ToggleLikeAsync(TestDbFactory.ApprovedSongId, TestDbFactory.ListenerId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Liked.Should().BeFalse();
        result.Data.NewCount.Should().Be(5); // back to seed
        db.Likes.Should().NotContain(l =>
            l.UserId == TestDbFactory.ListenerId && l.SongId == TestDbFactory.ApprovedSongId);
    }

    // ── Edge: like a song that doesn't exist ────────────────────────────────
    [Fact]
    public async Task ToggleLike_WhenSongMissing_ReturnsFailure()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ListenerId, Role.Listener));

        var result = await svc.ToggleLikeAsync(songId: 9999, userId: TestDbFactory.ListenerId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không tìm thấy");
    }

    // ── TC-17 — Edit title resets status to Pending ─────────────────────────
    [Fact]
    public async Task Update_WhenTitleChanges_ResetsStatusToPending()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ArtistAId, Role.Artist));

        var dto = new UpdateSongDto(Title: "New Title", CoverImage: null, AlbumId: null);
        var result = await svc.UpdateAsync(TestDbFactory.ApprovedSongId, dto, TestDbFactory.ArtistAId);

        result.IsSuccess.Should().BeTrue();
        var updated = await db.Songs.FindAsync(TestDbFactory.ApprovedSongId);
        updated!.Title.Should().Be("New Title");
        updated.Status.Should().Be(SongStatus.Pending); // reset rule
    }

    // ── TC-16 — Artist A cannot edit Artist B's song ────────────────────────
    [Fact]
    public async Task Update_WhenArtistDoesNotOwnSong_ReturnsFailure()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ArtistAId, Role.Artist));

        var dto = new UpdateSongDto(Title: "Hijack", CoverImage: null, AlbumId: null);
        var result = await svc.UpdateAsync(TestDbFactory.ArtistBSongId, dto, TestDbFactory.ArtistAId);

        result.IsSuccess.Should().BeFalse();
        var unchanged = await db.Songs.FindAsync(TestDbFactory.ArtistBSongId);
        unchanged!.Title.Should().Be("Artist B Track"); // not mutated
    }

    // ── TC-20 — Cross-owner album rejection in Create ───────────────────────
    [Fact]
    public async Task Create_WhenAlbumBelongsToAnotherArtist_ReturnsFailure()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ArtistAId, Role.Artist));

        var dto = new CreateSongDto("Track", "url.mp3", null, AlbumId: TestDbFactory.ArtistBAlbumId);
        var result = await svc.CreateAsync(dto, artistId: TestDbFactory.ArtistAId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Album");
        db.Songs.Should().NotContain(s => s.Title == "Track");
    }

    // ── Create happy path ───────────────────────────────────────────────────
    [Fact]
    public async Task Create_WithValidData_PersistsSongAsPending()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ArtistAId, Role.Artist));

        var dto = new CreateSongDto("Brand New", "audio.mp3", "cover.jpg", null);
        var result = await svc.CreateAsync(dto, TestDbFactory.ArtistAId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(SongStatus.Pending);
        result.Data.Title.Should().Be("Brand New");
    }

    // ── TC-18 — Search returns only Approved + matches title/artist ─────────
    [Fact]
    public async Task Search_OnlyReturnsApprovedSongs_MatchedByTitle()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ListenerId, Role.Listener));

        var result = await svc.SearchSongsAsync("song", TestDbFactory.ListenerId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().OnlyContain(s => s.Status == SongStatus.Approved);
        result.Data.Should().Contain(s => s.Id == TestDbFactory.ApprovedSongId);
        result.Data.Should().NotContain(s => s.Id == TestDbFactory.PendingSongId);
    }

    // ── TC-19 — Search empty when no match ──────────────────────────────────
    [Fact]
    public async Task Search_WhenNoMatch_ReturnsEmptyList()
    {
        using var db = TestDbFactory.Create();
        var svc = new SongService(db, FakeCurrentUser.As(TestDbFactory.ListenerId, Role.Listener));

        var result = await svc.SearchSongsAsync("zzz-no-match-zzz", TestDbFactory.ListenerId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().BeEmpty();
    }
}
