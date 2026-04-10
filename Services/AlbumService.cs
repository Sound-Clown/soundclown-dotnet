using Microsoft.EntityFrameworkCore;
using MusicApp.Data;
using MusicApp.DTOs;
using MusicApp.Entities;

namespace MusicApp.Services;

public class AlbumService : IAlbumService
{
    private readonly AppDbContext _db;

    public AlbumService(AppDbContext db) => _db = db;

    public async Task<ServiceResult<AlbumDto>> GetByIdAsync(int id)
    {
        var album = await _db.Albums
            .AsNoTracking()
            .Include(a => a.Artist)
            .Include(a => a.Songs)
                .ThenInclude(s => s.Artist)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (album == null)
            return ServiceResult<AlbumDto>.Fail("Không tìm thấy album.");

        return ServiceResult<AlbumDto>.Ok(MapAlbum(album));
    }

    public async Task<ServiceResult<List<AlbumListDto>>> GetArtistAlbumsAsync(int artistId)
    {
        var albums = await _db.Albums
            .AsNoTracking()
            .Where(a => a.ArtistId == artistId)
            .Include(a => a.Artist)
            .Include(a => a.Songs)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return ServiceResult<List<AlbumListDto>>.Ok(albums.Select(MapAlbumList).ToList());
    }

    public async Task<ServiceResult<AlbumDto>> CreateAsync(CreateAlbumDto dto, int artistId)
    {
        var album = new Album
        {
            Name = dto.Name.Trim(),
            CoverImage = dto.CoverImage,
            ArtistId = artistId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Albums.Add(album);
        await _db.SaveChangesAsync();

        var saved = await _db.Albums
            .Include(a => a.Artist).Include(a => a.Songs).ThenInclude(s => s.Artist)
            .FirstAsync(a => a.Id == album.Id);

        return ServiceResult<AlbumDto>.Ok(MapAlbum(saved));
    }

    public async Task<ServiceResult<AlbumDto>> UpdateAsync(int id, UpdateAlbumDto dto, int artistId)
    {
        var album = await _db.Albums
            .Include(a => a.Artist).Include(a => a.Songs).ThenInclude(s => s.Artist)
            .FirstOrDefaultAsync(a => a.Id == id && a.ArtistId == artistId);

        if (album == null)
            return ServiceResult<AlbumDto>.Fail("Không tìm thấy album hoặc bạn không có quyền.");

        if (dto.Name != null) album.Name = dto.Name.Trim();
        if (dto.CoverImage != null) album.CoverImage = dto.CoverImage;

        await _db.SaveChangesAsync();
        return ServiceResult<AlbumDto>.Ok(MapAlbum(album));
    }

    public async Task<ServiceResult> DeleteAsync(int id, int artistId)
    {
        var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == id && a.ArtistId == artistId);
        if (album == null)
            return ServiceResult.Fail("Không tìm thấy album hoặc bạn không có quyền.");

        _db.Albums.Remove(album);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AddSongAsync(int albumId, int songId, int artistId)
    {
        var album = await _db.Albums
            .Include(a => a.Songs)
            .FirstOrDefaultAsync(a => a.Id == albumId && a.ArtistId == artistId);

        if (album == null)
            return ServiceResult.Fail("Album không hợp lệ.");

        var song = await _db.Songs
            .FirstOrDefaultAsync(s => s.Id == songId && s.ArtistId == artistId);

        if (song == null)
            return ServiceResult.Fail("Bài hát không hợp lệ.");

        if (song.AlbumId == albumId)
            return ServiceResult.Ok(); // Already added

        song.AlbumId = albumId;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveSongAsync(int albumId, int songId, int artistId)
    {
        var album = await _db.Albums
            .FirstOrDefaultAsync(a => a.Id == albumId && a.ArtistId == artistId);

        if (album == null)
            return ServiceResult.Fail("Album không hợp lệ.");

        var song = await _db.Songs
            .FirstOrDefaultAsync(s => s.Id == songId && s.AlbumId == albumId && s.ArtistId == artistId);

        if (song == null)
            return ServiceResult.Fail("Bài hát không thuộc album này.");

        song.AlbumId = null;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    private AlbumDto MapAlbum(Album a) =>
        new(a.Id, a.Name, a.CoverImage, a.ArtistId, a.Artist.Username,
            a.Songs.Count, a.CreatedAt,
            a.Songs.OrderBy(s => s.CreatedAt).Select(s => new SongDto(
                s.Id, s.Title, s.AudioFile, s.CoverImage,
                s.ArtistId, s.Artist.Username,
                s.AlbumId, a.Name,
                s.Status, s.RejectReason,
                s.PlayCount, s.LikeCount, s.CreatedAt, false)).ToList());

    private AlbumListDto MapAlbumList(Album a) =>
        new(a.Id, a.Name, a.CoverImage, a.ArtistId, a.Artist.Username,
            a.Songs.Count, a.CreatedAt);
}
