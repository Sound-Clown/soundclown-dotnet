using MusicApp.Enums;

namespace MusicApp.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    Role? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsArtist { get; }
}
