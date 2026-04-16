using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.DTOs;
using MusicApp.Enums;
using MusicApp.Services;

namespace MusicApp.Controllers;

/// <summary>
/// API endpoints cho việc kiểm thử (Test). Các endpoint này phục vụ
/// cho việc thực hiện testcase TC-03, TC-04, TC-05, TC-11, TC-12, TC-15, TC-16, TC-20.
/// </summary>
[ApiController]
[Route("api")]
[Authorize] // yêu cầu đăng nhập
public class TestApiController : ControllerBase
{
    private readonly ISongService _songService;
    private readonly IAlbumService _albumService;
    private readonly IAdminService _adminService;
    private readonly ICurrentUserService _currentUser;

    public TestApiController(
        ISongService songService,
        IAlbumService albumService,
        IAdminService adminService,
        ICurrentUserService currentUser)
    {
        _songService = songService;
        _albumService = albumService;
        _adminService = adminService;
        _currentUser = currentUser;
    }

    // ═══════════════════════════════════════════════════════════════
    // LIKE / UNLIKE — TC-03, TC-04
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// POST /api/songs/{id}/like — Like hoặc unlike bài hát.
    /// TC-03: nhánh Like (chưa like → insert)
    /// TC-04: nhánh Unlike (đã like → delete)
    /// </summary>
    [HttpPost("songs/{id:int}/like")]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized(new { error = "Chưa đăng nhập." });

        var result = await _songService.ToggleLikeAsync(id, userId.Value);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    // ═══════════════════════════════════════════════════════════════
    // ADMIN — TC-05, TC-11, TC-12
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// GET /api/admin/songs — Lấy danh sách bài chờ duyệt.
    /// TC-05: phân quyền — Listener/Artist gọi → 403
    /// </summary>
    [HttpGet("admin/songs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingSongs()
    {
        var result = await _adminService.GetPendingSongsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// POST /api/admin/songs/{id}/review — Duyệt hoặc từ chối bài hát.
    /// TC-05: Listener/Artist gọi → 403 (route-level)
    /// TC-11: nhánh Approve (action = "approve")
    /// TC-12: nhánh Reject (action = "reject")
    /// </summary>
    [HttpPost("admin/songs/{id:int}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReviewSong(int id, [FromBody] ReviewSongDto dto)
    {
        var result = await _adminService.ReviewSongAsync(id, dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Thành công.", action = dto.Action.ToLowerInvariant() });
    }

    /// <summary>
    /// POST /api/admin/users/{id}/toggle-lock — Khóa hoặc mở khóa tài khoản.
    /// TC-14: Admin tự khóa mình → service trả lỗi
    /// TC-15: nhánh hợp lệ (targetId != adminId)
    /// </summary>
    [HttpPost("admin/users/{id:int}/toggle-lock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleLockUser(int id)
    {
        var adminId = _currentUser.UserId!.Value;
        var result = await _adminService.ToggleLockUserAsync(id, adminId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var user = (await _adminService.GetUsersAsync(1, 100)).Data?.Items
            .FirstOrDefault(u => u.Id == id);
        return Ok(new {
            message = user?.IsActive == false ? "Đã khóa." : "Đã mở khóa.",
            isActive = user?.IsActive
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // SONG UPDATE — TC-16
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// PUT /api/songs/{id} — Cập nhật bài hát (title, coverImage, albumId).
    /// TC-16: Artist A sửa bài Artist B → 403 (service-level check)
    /// TC-17: Sửa title bài Approved → status reset về Pending
    /// </summary>
    [HttpPut("songs/{id:int}")]
    public async Task<IActionResult> UpdateSong(int id, [FromBody] UpdateSongDto dto)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized(new { error = "Chưa đăng nhập." });

        var result = await _songService.UpdateAsync(id, dto, userId.Value);
        if (!result.IsSuccess)
            return StatusCode(403, new { error = result.Error });

        return Ok(result.Data);
    }

    // ═══════════════════════════════════════════════════════════════
    // ALBUM ADD SONG — TC-20
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// POST /api/albums/{id}/songs — Thêm bài hát vào album.
    /// TC-20: Artist thêm bài của người khác → service trả lỗi
    /// </summary>
    [HttpPost("albums/{id:int}/songs")]
    public async Task<IActionResult> AddSongToAlbum(int id, [FromBody] AddSongToAlbumDto dto)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized(new { error = "Chưa đăng nhập." });

        var result = await _albumService.AddSongAsync(id, dto.SongId, userId.Value);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Đã thêm bài hát vào album." });
    }

    /// <summary>
    /// GET /api/songs — Danh sách bài hát đã duyệt (lấy ID cho test).
    /// Dùng để query songId trước khi test các API khác.
    /// </summary>
    [HttpGet("songs")]
    public async Task<IActionResult> GetSongs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _songService.GetApprovedSongsAsync(page, pageSize);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// GET /api/albums — Danh sách album của user hiện tại (lấy albumId cho test).
    /// </summary>
    [HttpGet("albums")]
    public async Task<IActionResult> GetMyAlbums()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized(new { error = "Chưa đăng nhập." });

        var result = await _albumService.GetArtistAlbumsAsync(userId.Value);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Data);
    }
}

public record AddSongToAlbumDto(int SongId);