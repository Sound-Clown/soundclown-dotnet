using Microsoft.EntityFrameworkCore;
using MusicApp.Data;
using MusicApp.DTOs;
using MusicApp.Entities;
using MusicApp.Enums;

namespace MusicApp.Services;

public class SongService : ISongService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SongService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResult<SongDto>>> GetApprovedSongsAsync(int page, int pageSize = 20)
    {
        var query = _db.Songs
            .AsNoTracking()
            .Where(s => s.Status == SongStatus.Approved)
            .Include(s => s.Artist)
            .OrderByDescending(s => s.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(s => MapSong(s, _currentUser.UserId)).ToList();
        var paged = new PagedResult<SongDto>(dtos, total, page, pageSize);
        return ServiceResult<PagedResult<SongDto>>.Ok(paged);
    }

    public async Task<ServiceResult<List<SongDto>>> SearchSongsAsync(string query, int currentUserId)
    {
        var q = query.Trim().ToLower();
        var songs = await _db.Songs
            .AsNoTracking()
            .Where(s => s.Status == SongStatus.Approved &&
                        (s.Title.ToLower().Contains(q) ||
                         s.Artist.Username.ToLower().Contains(q)))
            .Include(s => s.Artist)
            .OrderBy(s => s.Title)
            .Take(50)
            .ToListAsync();

        return ServiceResult<List<SongDto>>.Ok(
            songs.Select(s => MapSong(s, currentUserId)).ToList());
    }

    public async Task<ServiceResult<SongDto>> GetByIdAsync(int id, int currentUserId)
    {
        var song = await _db.Songs
            .AsNoTracking()
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (song == null)
            return ServiceResult<SongDto>.Fail("Không tìm thấy bài hát.");

        return ServiceResult<SongDto>.Ok(MapSong(song, currentUserId));
    }

    public async Task<ServiceResult<SongDto>> CreateAsync(CreateSongDto dto, int artistId)
    {
        if (dto.AlbumId.HasValue)
        {
            var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == dto.AlbumId && a.ArtistId == artistId);
            if (album == null)
                return ServiceResult<SongDto>.Fail("Album không hợp lệ.");
        }

        var song = new Song
        {
            Title = dto.Title.Trim(),
            AudioFile = dto.AudioFile,
            CoverImage = dto.CoverImage,
            ArtistId = artistId,
            AlbumId = dto.AlbumId,
            Status = SongStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Songs.Add(song);
        await _db.SaveChangesAsync();

        // Reload with navigation
        var saved = await _db.Songs
            .Include(s => s.Artist).Include(s => s.Album).FirstAsync(s => s.Id == song.Id);

        return ServiceResult<SongDto>.Ok(MapSong(saved, artistId));
    }

    public async Task<ServiceResult<SongDto>> UpdateAsync(int id, UpdateSongDto dto, int artistId)
    {
        var song = await _db.Songs
            .Include(s => s.Artist).Include(s => s.Album)
            .FirstOrDefaultAsync(s => s.Id == id && s.ArtistId == artistId);

        if (song == null)
            return ServiceResult<SongDto>.Fail("Không tìm thấy bài hát hoặc bạn không có quyền.");

        var resetStatus = false;
        if (dto.Title != null && dto.Title != song.Title)
        {
            song.Title = dto.Title.Trim();
            resetStatus = true;
        }
        if (dto.CoverImage != null && dto.CoverImage != song.CoverImage)
        {
            song.CoverImage = dto.CoverImage;
            resetStatus = true;
        }
        if (dto.AlbumId.HasValue || dto.AlbumId == null)
        {
            song.AlbumId = dto.AlbumId;
        }

        if (resetStatus)
            song.Status = SongStatus.Pending;

        await _db.SaveChangesAsync();
        return ServiceResult<SongDto>.Ok(MapSong(song, artistId));
    }

    public async Task<ServiceResult> DeleteAsync(int id, int artistId)
    {
        var song = await _db.Songs.FirstOrDefaultAsync(s => s.Id == id && s.ArtistId == artistId);
        if (song == null)
            return ServiceResult.Fail("Không tìm thấy bài hát hoặc bạn không có quyền.");

        _db.Songs.Remove(song);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> IncrementPlayCountAsync(int id)
    {
        var song = await _db.Songs.FindAsync(id);
        if (song == null) return ServiceResult.Fail("Không tìm thấy bài hát.");
        song.PlayCount++;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<LikeResult>> ToggleLikeAsync(int songId, int userId)
    {
        var song = await _db.Songs.FindAsync(songId);
        if (song == null) return ServiceResult<LikeResult>.Fail("Không tìm thấy bài hát.");

        var existing = await _db.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.SongId == songId);

        if (existing != null)
        {
            _db.Likes.Remove(existing);
            song.LikeCount--;
            await _db.SaveChangesAsync();
            return ServiceResult<LikeResult>.Ok(new LikeResult(false, song.LikeCount));
        }

        _db.Likes.Add(new Like { UserId = userId, SongId = songId, CreatedAt = DateTime.UtcNow });
        song.LikeCount++;
        await _db.SaveChangesAsync();
        return ServiceResult<LikeResult>.Ok(new LikeResult(true, song.LikeCount));
    }

    public async Task<ServiceResult<List<SongDto>>> GetArtistSongsAsync(int artistId)
    {
        var songs = await _db.Songs
            .AsNoTracking()
            .Where(s => s.ArtistId == artistId)
            .Include(s => s.Artist).Include(s => s.Album)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return ServiceResult<List<SongDto>>.Ok(
            songs.Select(s => MapSong(s, artistId)).ToList());
    }

    public async Task<ServiceResult<StatsDto>> GetArtistStatsAsync(int artistId)
    {
        var songs = await _db.Songs
            .AsNoTracking()
            .Where(s => s.ArtistId == artistId && s.Status == SongStatus.Approved)
            .OrderByDescending(s => s.PlayCount)
            .ToListAsync();

        var totalPlays = songs.Sum(s => s.PlayCount);
        var totalLikes = songs.Sum(s => s.LikeCount);
        var tracks = songs.Select(s => new TrackStatDto(
            s.Id, s.Title, s.CoverImage, s.PlayCount, s.LikeCount)).ToList();

        return ServiceResult<StatsDto>.Ok(new StatsDto(totalPlays, totalLikes, tracks));
    }

    private SongDto MapSong(Song s, int? currentUserId)
    {
        var isLiked = currentUserId.HasValue &&
            _db.Likes.Any(l => l.UserId == currentUserId.Value && l.SongId == s.Id);
        return new SongDto(
            s.Id, s.Title, s.AudioFile, s.CoverImage,
            s.ArtistId, s.Artist.Username,
            s.AlbumId, s.Album?.Name,
            s.Status, s.RejectReason,
            s.PlayCount, s.LikeCount,
            s.CreatedAt, isLiked);
    }
}
