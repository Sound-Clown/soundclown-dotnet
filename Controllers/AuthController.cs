using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicApp.Data;
using MusicApp.DTOs;
using MusicApp.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

    private string GenerateJwtToken(User user)
    {
        var jwt = _config.GetSection("Auth:Jwt");
        var key = jwt["SecretKey"] ?? "SoundClown-TestSecretKey-Minimum32Chars!";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
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
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto? dto)
    {
        // If called with JSON body (API/Postman), return JSON token
        // If called with form data (browser), set cookie + redirect
        if (dto != null)
        {
            // JSON request — API caller (Postman / mobile)
            if (string.IsNullOrWhiteSpace(dto.Identifier) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = "Vui lòng nhập đầy đủ thông tin." });

            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == dto.Identifier.ToLowerInvariant() ||
                    u.Username == dto.Identifier);

            if (user == null)
                return Unauthorized(new { error = "Tài khoản không tồn tại." });

            if (!user.IsActive)
                return Unauthorized(new { error = "Tài khoản đã bị khóa." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { error = "Mật khẩu không đúng." });

            // Set cookie (still needed for browser session)
            await SignInAsync(user);
            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Id, user.Username, user.Email, Role = user.Role.ToString() } });
        }

        // Form data — browser login
        var form = Request.Form;
        var identifier = form["identifier"].ToString();
        var password = form["password"].ToString();

        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            return Redirect($"/login?error={Uri.EscapeDataString("Vui lòng nhập đầy đủ thông tin.")}");

        var u = await _db.Users
            .FirstOrDefaultAsync(x =>
                x.Email == identifier.ToLowerInvariant() ||
                x.Username == identifier);

        if (u == null)
            return Redirect($"/login?error={Uri.EscapeDataString("Tài khoản không tồn tại.")}");

        if (!u.IsActive)
            return Redirect($"/login?error={Uri.EscapeDataString("Tài khoản đã bị khóa.")}");

        if (!BCrypt.Net.BCrypt.Verify(password, u.PasswordHash))
            return Redirect($"/login?error={Uri.EscapeDataString("Mật khẩu không đúng.")}");

        await SignInAsync(u);
        return Redirect("/");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Đã đăng xuất." });
    }
}
