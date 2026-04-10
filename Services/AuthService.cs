using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data;
using MusicApp.DTOs;
using MusicApp.Entities;
using MusicApp.Enums;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MusicApp.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContext;

    public AuthService(AppDbContext db, IConfiguration config, IHttpContextAccessor httpContext)
    {
        _db = db;
        _config = config;
        _httpContext = httpContext;
    }

    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return ServiceResult<UserDto>.Fail("Email đã được sử dụng.");

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return ServiceResult<UserDto>.Fail("Username đã được sử dụng.");

        if (!Enum.TryParse<Role>(dto.Role, true, out var role) || role == Role.Admin)
            return ServiceResult<UserDto>.Fail("Role không hợp lệ.");

        var user = new User
        {
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return ServiceResult<UserDto>.Ok(MapUser(user));
    }

    public async Task<ServiceResult<UserDto>> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.Email == dto.Identifier.ToLowerInvariant() ||
                u.Username == dto.Identifier);

        if (user == null)
            return ServiceResult<UserDto>.Fail("Tài khoản không tồn tại.");

        if (!user.IsActive)
            return ServiceResult<UserDto>.Fail("Tài khoản đã bị khóa.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ServiceResult<UserDto>.Fail("Mật khẩu không đúng.");

        await SignInAsync(user);
        return ServiceResult<UserDto>.Ok(MapUser(user));
    }

    public async Task LogoutAsync()
    {
        await _httpContext.HttpContext!.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<ServiceResult> ForgotPasswordAsync(string email)
    {
        // Always return success — don't reveal whether account exists
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        if (user == null) return ServiceResult.Ok();

        // Generate token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var reset = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };

        // Replace any existing token
        var existing = await _db.PasswordResetTokens.Where(t => t.UserId == user.Id).ToListAsync();
        _db.PasswordResetTokens.RemoveRange(existing);
        _db.PasswordResetTokens.Add(reset);
        await _db.SaveChangesAsync();

        // TODO: Send email via MailKit (configure SMTP in appsettings.json)
        // Email sending is pending SMTP setup in appsettings.json

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var reset = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == dto.Token);

        if (reset == null)
            return ServiceResult.Fail("Token không hợp lệ.");

        if (reset.ExpiresAt < DateTime.UtcNow)
            return ServiceResult.Fail("Token đã hết hạn. Vui lòng yêu cầu lại.");

        reset.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        _db.PasswordResetTokens.Remove(reset);
        await _db.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return ServiceResult.Fail("Không tìm thấy người dùng.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return ServiceResult.Fail("Mật khẩu hiện tại không đúng.");

        if (dto.NewPassword != dto.ConfirmPassword)
            return ServiceResult.Fail("Xác nhận mật khẩu không khớp.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private async Task SignInAsync(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(
                int.Parse(_config["Auth:ExpireDays"] ?? "7"))
        };

        await _httpContext.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
    }

    private static UserDto MapUser(User u) =>
        new(u.Id, u.Username, u.Email, u.Role, u.IsActive, u.CreatedAt);
}
