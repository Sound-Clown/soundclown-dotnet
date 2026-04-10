namespace MusicApp.DTOs;

public record StatsDto(
    int TotalPlays,
    int TotalLikes,
    List<TrackStatDto> Tracks
);

public record TrackStatDto(
    int SongId,
    string Title,
    string? CoverImage,
    int PlayCount,
    int LikeCount
);

public record LikeResult(bool Liked, int NewCount);
