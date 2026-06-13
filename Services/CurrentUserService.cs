using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MusicApp.Enums;

namespace MusicApp.Services;

public sealed class CurrentUserService : ICurrentUserService, IDisposable
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal? _user;

    public CurrentUserService(
        AuthenticationStateProvider authProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _authProvider = authProvider;
        _httpContextAccessor = httpContextAccessor;
        _authProvider.AuthenticationStateChanged += OnAuthChanged;
        TrySyncUser();
    }

    private void OnAuthChanged(Task<AuthenticationState> _)
    {
        _user = null;
    }

    private void TrySyncUser()
    {
        try
        {
            _user = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult().User;
        }
        catch (InvalidOperationException)
        {
            // Ngoài Blazor circuit (API controller) — ServerAuthenticationStateProvider
            // không dùng được; lấy principal từ HttpContext (cookie hoặc JWT bearer).
            _user = _httpContextAccessor.HttpContext?.User;
        }
    }

    private void EnsureSynced()
    {
        if (_user != null) return;
        TrySyncUser();
    }

    public int? UserId
    {
        get
        {
            EnsureSynced();
            var val = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return val != null && int.TryParse(val, out var id) ? id : null;
        }
    }

    public string? Username
    {
        get { EnsureSynced(); return _user?.FindFirst(ClaimTypes.Name)?.Value; }
    }

    public Role? Role
    {
        get
        {
            EnsureSynced();
            var val = _user?.FindFirst(ClaimTypes.Role)?.Value;
            return val != null && Enum.TryParse<Role>(val, out var r) ? r : null;
        }
    }

    public bool IsAuthenticated
    {
        get { EnsureSynced(); return _user?.Identity?.IsAuthenticated ?? false; }
    }

    public bool IsAdmin => Role == Enums.Role.Admin;
    public bool IsArtist => Role == Enums.Role.Artist;

    public void Dispose()
    {
        _authProvider.AuthenticationStateChanged -= OnAuthChanged;
    }
}
