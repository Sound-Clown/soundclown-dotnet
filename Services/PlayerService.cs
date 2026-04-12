namespace MusicApp.Services;

public class PlayerService : IPlayerService
{
    public List<PlayerSong> Queue { get; private set; } = new();
    public int CurrentIndex { get; private set; }
    public bool IsPlaying { get; private set; }
    public string? Source { get; private set; }
    public PlayerSong? CurrentSong => Queue.Count > 0 && CurrentIndex >= 0 && CurrentIndex < Queue.Count
        ? Queue[CurrentIndex] : null;

    public void SetQueue(List<PlayerSong> songs, int startIndex, string source)
    {
        Queue = songs;
        CurrentIndex = startIndex;
        Source = source;
        IsPlaying = true;
        OnChange?.Invoke();
    }

    public void Next()
    {
        if (CurrentIndex < Queue.Count - 1)
        {
            CurrentIndex++;
            OnChange?.Invoke();
        }
    }

    public void Prev()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            OnChange?.Invoke();
        }
    }

    public void TogglePlay()
    {
        if (Queue.Count > 0)
        {
            IsPlaying = !IsPlaying;
            OnChange?.Invoke();
        }
    }

    public void SetPlaying(bool playing)
    {
        IsPlaying = playing;
        OnChange?.Invoke();
    }

    /// <summary>Call this to re-sync state after a new circuit/instance is created.</summary>
    public void CheckState() => OnChange?.Invoke();

    public event Action? OnChange;
}
