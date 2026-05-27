using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MusicApp.DTOs;
using MusicApp.Enums;
using MusicApp.Services;
using SoundClown.UnitTests.Helpers;

namespace SoundClown.UnitTests.Services;

/// <summary>
/// Unit tests for AdminService — focus on branches referenced in the report
/// (TC-05 privilege escalation, TC-11 approve branch, TC-12 reject branch,
/// TC-13 reject validation, TC-14/TC-15 self-lock guard).
/// </summary>
public class AdminServiceTests
{
    // ── TC-11 — Approve branch ──────────────────────────────────────────────
    [Fact]
    public async Task Review_AsAdmin_ApproveAction_SetsStatusApproved()
    {
        using var db = TestDbFactory.Create();
        var svc = new AdminService(db, FakeCurrentUser.As(TestDbFactory.AdminId, Role.Admin));

        var result = await svc.ReviewSongAsync(TestDbFactory.PendingSongId,
            new ReviewSongDto("approve", null));

        result.IsSuccess.Should().BeTrue();
        var song = await db.Songs.FindAsync(TestDbFactory.PendingSongId);
        song!.Status.Should().Be(SongStatus.Approved);
        song.RejectReason.Should().BeNull();
    }

    // ── TC-12 — Reject branch (with reason) ─────────────────────────────────
    [Fact]
    public async Task Review_AsAdmin_RejectActionWithReason_SetsStatusRejected()
    {
        using var db = TestDbFactory.Create();
        var svc = new AdminService(db, FakeCurrentUser.As(TestDbFactory.AdminId, Role.Admin));

        var result = await svc.ReviewSongAsync(TestDbFactory.PendingSongId,
            new ReviewSongDto("reject", "Chất lượng audio kém"));

        result.IsSuccess.Should().BeTrue();
        var song = await db.Songs.FindAsync(TestDbFactory.PendingSongId);
        song!.Status.Should().Be(SongStatus.Rejected);
        song.RejectReason.Should().Be("Chất lượng audio kém");
    }

    // ── TC-13 — Reject without reason → validation error ────────────────────
    [Fact]
    public async Task Review_RejectWithoutReason_ReturnsFailure_StatusUnchanged()
    {
        using var db = TestDbFactory.Create();
        var svc = new AdminService(db, FakeCurrentUser.As(TestDbFactory.AdminId, Role.Admin));

        var result = await svc.ReviewSongAsync(TestDbFactory.PendingSongId,
            new ReviewSongDto("reject", ""));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("lý do");
        var song = await db.Songs.FindAsync(TestDbFactory.PendingSongId);
        song!.Status.Should().Be(SongStatus.Pending); // unchanged
    }

    // ── TC-05 — Privilege escalation guard at service level ─────────────────
    [Theory]
    [InlineData(Role.Listener)]
    [InlineData(Role.Artist)]
    public async Task Review_AsNonAdmin_IsRejected(Role role)
    {
        using var db = TestDbFactory.Create();
        var svc = new AdminService(db, FakeCurrentUser.As(TestDbFactory.ListenerId, role));

        var result = await svc.ReviewSongAsync(TestDbFactory.PendingSongId,
            new ReviewSongDto("approve", null));

        result.IsSuccess.Should().BeFalse();
        var song = await db.Songs.FindAsync(TestDbFactory.PendingSongId);
        song!.Status.Should().Be(SongStatus.Pending); // unchanged
    }

    // ── TC-14 / TC-15 — Self-lock guard ─────────────────────────────────────
    [Fact]
    public async Task ToggleLockUser_WhenTargetIsSelf_ReturnsFailure()
    {
        using var db = TestDbFactory.Create();
        var svc = new AdminService(db, FakeCurrentUser.As(TestDbFactory.AdminId, Role.Admin));

        var result = await svc.ToggleLockUserAsync(
            targetUserId: TestDbFactory.AdminId,
            adminId: TestDbFactory.AdminId);

        result.IsSuccess.Should().BeFalse();
        var admin = await db.Users.FindAsync(TestDbFactory.AdminId);
        admin!.IsActive.Should().BeTrue();
    }

    // ── TC-15 — Lock another user toggles IsActive ──────────────────────────
    [Fact]
    public async Task ToggleLockUser_OnOtherUser_FlipsIsActive()
    {
        using var db = TestDbFactory.Create();
        var svc = new AdminService(db, FakeCurrentUser.As(TestDbFactory.AdminId, Role.Admin));

        var before = await db.Users.AsNoTracking().FirstAsync(u => u.Id == TestDbFactory.ListenerId);
        before.IsActive.Should().BeTrue();

        var result = await svc.ToggleLockUserAsync(
            targetUserId: TestDbFactory.ListenerId,
            adminId: TestDbFactory.AdminId);

        result.IsSuccess.Should().BeTrue();
        var after = await db.Users.AsNoTracking().FirstAsync(u => u.Id == TestDbFactory.ListenerId);
        after.IsActive.Should().BeFalse();
    }
}
