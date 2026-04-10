using Microsoft.EntityFrameworkCore;
using MusicApp.Data;
using MusicApp.DTOs;
using MusicApp.Enums;

namespace MusicApp.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db) => _db = db;

    public async Task<ServiceResult<List<SongDto>>> GetPendingSongsAsync()
    {
        var songs = await _db.Songs
            .AsNoTracking()
            .Where(s => s.Status == SongStatus.Pending)
            .Include(s => s.Artist).Include(s => s.Album)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return ServiceResult<List<SongDto>>.Ok(
            songs.Select(s => new SongDto(
                s.Id, s.Title, s.AudioFile, s.CoverImage,
                s.ArtistId, s.Artist.Username,
                s.AlbumId, s.Album?.Name,
                s.Status, s.RejectReason,
                s.PlayCount, s.LikeCount,
                s.CreatedAt, false)).ToList());
    }

    public async Task<ServiceResult> ReviewSongAsync(int songId, ReviewSongDto dto)
    {
        var song = await _db.Songs.FindAsync(songId);
        if (song == null)
            return ServiceResult.Fail("Không tìm thấy bài hát.");

        switch (dto.Action.ToLowerInvariant())
        {
            case "approve":
                song.Status = SongStatus.Approved;
                song.RejectReason = null;
                break;
            case "reject":
                if (string.IsNullOrWhiteSpace(dto.RejectReason))
                    return ServiceResult.Fail("Vui lòng nhập lý do từ chối.");
                song.Status = SongStatus.Rejected;
                song.RejectReason = dto.RejectReason;
                break;
            default:
                return ServiceResult.Fail("Hành động không hợp lệ.");
        }

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int page, int pageSize = 20)
    {
        var query = _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(u => new UserDto(
            u.Id, u.Username, u.Email, u.Role, u.IsActive, u.CreatedAt)).ToList();
        var paged = new PagedResult<UserDto>(dtos, total, page, pageSize);
        return ServiceResult<PagedResult<UserDto>>.Ok(paged);
    }

    public async Task<ServiceResult> ToggleLockUserAsync(int targetUserId, int adminId)
    {
        if (targetUserId == adminId)
            return ServiceResult.Fail("Không thể khóa tài khoản của chính mình.");

        var user = await _db.Users.FindAsync(targetUserId);
        if (user == null)
            return ServiceResult.Fail("Không tìm thấy người dùng.");

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
