using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data;

namespace MusicApp.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string identifier, string password)
    {
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            return Redirect($"/login?error={Uri.EscapeDataString("Vui lòng nhập đầy đủ thông tin.")}");

        var user = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.Email == identifier.ToLowerInvariant() ||
                u.Username == identifier);

        if (user == null)
            return Redirect($"/login?error={Uri.EscapeDataString("Tài khoản không tồn tại.")}");

        if (!user.IsActive)
            return Redirect($"/login?error={Uri.EscapeDataString("Tài khoản đã bị khóa.")}");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Redirect($"/login?error={Uri.EscapeDataString("Mật khẩu không đúng.")}");

        // Sign in — controller has full HttpContext ✅
        var claims = new[]
        {
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Name, user.Username),
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
        };

        var identity = new System.Security.Claims.ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(
                int.Parse(_config["Auth:ExpireDays"] ?? "7"))
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

        return Redirect("/");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}
