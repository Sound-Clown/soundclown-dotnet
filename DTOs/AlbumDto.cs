namespace MusicApp.DTOs;

public record AlbumDto(
    int Id,
    string Name,
    string? CoverImage,
    int ArtistId,
    string ArtistUsername,
    int SongCount,
    DateTime CreatedAt,
    List<SongDto> Songs
);

public record AlbumListDto(
    int Id,
    string Name,
    string? CoverImage,
    int ArtistId,
    string ArtistUsername,
    int SongCount,
    DateTime CreatedAt
);

public record CreateAlbumDto(
    string Name,
    string? CoverImage
);

public record UpdateAlbumDto(
    string? Name,
    string? CoverImage
);
