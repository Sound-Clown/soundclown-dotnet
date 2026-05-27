using MusicApp.Enums;
using MusicApp.Services;

namespace SoundClown.UnitTests.Helpers;

public class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; init; }
    public string? Username { get; init; }
    public Role? Role { get; init; }
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsAdmin => Role == MusicApp.Enums.Role.Admin;
    public bool IsArtist => Role == MusicApp.Enums.Role.Artist;

    public static FakeCurrentUser As(int id, Role role) => new() { UserId = id, Role = role, Username = $"user{id}" };
    public static FakeCurrentUser Anonymous() => new();
}
