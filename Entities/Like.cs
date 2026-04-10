namespace MusicApp.Entities;

public class Like
{
    public int UserId { get; set; }
    public int SongId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Song Song { get; set; } = null!;
}
