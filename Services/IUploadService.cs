// @ Services/IUploadService.cs
using MusicApp.DTOs;
using AppUploadResult = MusicApp.DTOs.UploadResult;

namespace MusicApp.Services;

public interface IUploadService
{
    Task<ServiceResult<AppUploadResult>> UploadAudioAsync(Stream stream, string fileName, string contentType, long length);
    Task<ServiceResult<AppUploadResult>> UploadImageAsync(Stream stream, string fileName, string contentType, long length);
    Task<ServiceResult> DeleteFileAsync(string publicId);
}
