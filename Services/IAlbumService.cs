using MusicApp.DTOs;

namespace MusicApp.Services;

public interface IAlbumService
{
    Task<ServiceResult<AlbumDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<AlbumListDto>>> GetArtistAlbumsAsync(int artistId);
    Task<ServiceResult<AlbumDto>> CreateAsync(CreateAlbumDto dto, int artistId);
    Task<ServiceResult<AlbumDto>> UpdateAsync(int id, UpdateAlbumDto dto, int artistId);
    Task<ServiceResult> DeleteAsync(int id, int artistId);
    Task<ServiceResult> AddSongAsync(int albumId, int songId, int artistId);
    Task<ServiceResult> RemoveSongAsync(int albumId, int songId, int artistId);
}
