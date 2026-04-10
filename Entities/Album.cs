// @ Entities/Album.cs
namespace MusicApp.Entities;

public class Album
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CoverImage { get; set; }
    public int ArtistId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Artist { get; set; } = null!;
    public ICollection<Song> Songs { get; set; } = new List<Song>();
}
