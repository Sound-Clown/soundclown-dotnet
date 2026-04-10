using MusicApp.Enums;

namespace MusicApp.DTOs;

public record SongDto(
    int Id,
    string Title,
    string AudioFile,
    string? CoverImage,
    int ArtistId,
    string ArtistUsername,
    int? AlbumId,
    string? AlbumName,
    SongStatus Status,
    string? RejectReason,
    int PlayCount,
    int LikeCount,
    DateTime CreatedAt,
    bool IsLiked
);

public record CreateSongDto(
    string Title,
    string AudioFile,
    string? CoverImage,
    int? AlbumId
);

public record UpdateSongDto(
    string? Title,
    string? CoverImage,
    int? AlbumId
);

public record ReviewSongDto(
    string Action, // "approve" or "reject"
    string? RejectReason
);
