using MusicApp.DTOs;

namespace MusicApp.Services;

public interface IAdminService
{
    Task<ServiceResult<List<SongDto>>> GetPendingSongsAsync();
    Task<ServiceResult> ReviewSongAsync(int songId, ReviewSongDto dto);
    Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int page, int pageSize = 20);
    Task<ServiceResult> ToggleLockUserAsync(int targetUserId, int adminId);
}
