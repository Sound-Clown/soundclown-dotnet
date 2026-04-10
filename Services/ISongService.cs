// @ Services/ISongService.cs
using MusicApp.DTOs;

namespace MusicApp.Services;

public interface ISongService
{
    Task<ServiceResult<PagedResult<SongDto>>> GetApprovedSongsAsync(int page, int pageSize = 20);
    Task<ServiceResult<List<SongDto>>> SearchSongsAsync(string query, int currentUserId);
    Task<ServiceResult<SongDto>> GetByIdAsync(int id, int currentUserId);
    Task<ServiceResult<SongDto>> CreateAsync(CreateSongDto dto, int artistId);
    Task<ServiceResult<SongDto>> UpdateAsync(int id, UpdateSongDto dto, int artistId);
    Task<ServiceResult> DeleteAsync(int id, int artistId);
    Task<ServiceResult> IncrementPlayCountAsync(int id);
    Task<ServiceResult<LikeResult>> ToggleLikeAsync(int songId, int userId);
    Task<ServiceResult<List<SongDto>>> GetArtistSongsAsync(int artistId);
    Task<ServiceResult<StatsDto>> GetArtistStatsAsync(int artistId);
}
