// @ Entities/Song.cs
using MusicApp.Enums;

namespace MusicApp.Entities;

public class Song
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AudioFile { get; set; } = string.Empty;
    public string? CoverImage { get; set; }
    public int ArtistId { get; set; }
    public int? AlbumId { get; set; }
    public SongStatus Status { get; set; } = SongStatus.Pending;
    public string? RejectReason { get; set; }
    public int PlayCount { get; set; }
    public int LikeCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Artist { get; set; } = null!;
    public Album? Album { get; set; }
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
