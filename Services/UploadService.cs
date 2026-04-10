using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using MusicApp.DTOs;
using AppUploadResult = MusicApp.DTOs.UploadResult;

namespace MusicApp.Services;

public class UploadService : IUploadService
{
    private readonly Cloudinary _cloudinary;

    public UploadService(IConfiguration config)
    {
        var c = config.GetSection("Cloudinary");
        _cloudinary = new Cloudinary(new Account(
            c["CloudName"],
            c["ApiKey"],
            c["ApiSecret"]));
    }

    public async Task<ServiceResult<AppUploadResult>> UploadAudioAsync(Stream stream, string fileName, string contentType, long length)
    {
        var validation = ValidateAudio(contentType, length);
        if (!validation.IsSuccess)
            return ServiceResult<AppUploadResult>.Fail(validation.Error!);

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, ms),
            Folder = "music-app/audio"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
            return ServiceResult<AppUploadResult>.Fail("Upload thất bại: " + result.Error.Message);

        return ServiceResult<AppUploadResult>.Ok(new AppUploadResult(result.SecureUrl.ToString(), result.PublicId));
    }

    public async Task<ServiceResult<AppUploadResult>> UploadImageAsync(Stream stream, string fileName, string contentType, long length)
    {
        var validation = ValidateImage(contentType, length);
        if (!validation.IsSuccess)
            return ServiceResult<AppUploadResult>.Fail(validation.Error!);

        using var msImg = new MemoryStream();
        await stream.CopyToAsync(msImg);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, msImg),
            Folder = "music-app/covers",
            Transformation = new Transformation().Width(600).Height(600).Crop("fill")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
            return ServiceResult<AppUploadResult>.Fail("Upload thất bại: " + result.Error.Message);

        return ServiceResult<AppUploadResult>.Ok(new AppUploadResult(result.SecureUrl.ToString(), result.PublicId));
    }

    public async Task<ServiceResult> DeleteFileAsync(string publicId)
    {
        var result = await _cloudinary.DeleteResourcesAsync(publicId);
        if (result.Error != null)
            return ServiceResult.Fail("Xóa file thất bại: " + result.Error.Message);
        return ServiceResult.Ok();
    }

    private static ServiceResult ValidateAudio(string contentType, long length)
    {
        if (length > 10 * 1024 * 1024)
            return ServiceResult.Fail("File âm thanh tối đa 10MB.");
        if (contentType != "audio/mpeg" && contentType != "audio/mp3")
            return ServiceResult.Fail("Chỉ chấp nhận file MP3.");
        return ServiceResult.Ok();
    }

    private static ServiceResult ValidateImage(string contentType, long length)
    {
        if (length > 2 * 1024 * 1024)
            return ServiceResult.Fail("Ảnh bìa tối đa 2MB.");
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(contentType.ToLowerInvariant()))
            return ServiceResult.Fail("Chỉ chấp nhận ảnh JPG, PNG hoặc WebP.");
        return ServiceResult.Ok();
    }
}
