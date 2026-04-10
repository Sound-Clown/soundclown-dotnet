// @ Services/IAuthService.cs
using MusicApp.DTOs;

namespace MusicApp.Services;

public interface IAuthService
{
    Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<UserDto>> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<ServiceResult> ForgotPasswordAsync(string email);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
