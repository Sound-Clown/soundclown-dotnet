namespace MusicApp.Services;

public record PlayerSong(int Id, string Title, string? CoverImage, string AudioUrl, string ArtistUsername, int PlayCount, int LikeCount, bool IsLiked);

public interface IPlayerService
{
    List<PlayerSong> Queue { get; }
    int CurrentIndex { get; }
    bool IsPlaying { get; }
    PlayerSong? CurrentSong { get; }
    string? Source { get; }

    void SetQueue(List<PlayerSong> songs, int startIndex, string source);
    void Next();
    void Prev();
    void TogglePlay();
    void SetPlaying(bool playing);
    event Action? OnChange;
}
