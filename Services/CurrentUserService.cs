// @ Services/CurrentUserService.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MusicApp.Enums;

namespace MusicApp.Services;

public class CurrentUserService : ICurrentUserService, IDisposable
{
    private readonly AuthenticationStateProvider _authProvider;
    private AuthenticationState? _lastState;
    private ClaimsPrincipal? _user;

    public CurrentUserService(AuthenticationStateProvider authProvider)
    {
        _authProvider = authProvider;
        _authProvider.AuthenticationStateChanged += OnAuthChanged;
        SyncUser();
    }

    private void OnAuthChanged(Task<AuthenticationState> _)
    {
        _user = null;
        _lastState = null;
    }

    private void SyncUser()
    {
        _lastState = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        _user = _lastState.User;
    }

    private void EnsureSynced()
    {
        if (_user != null) return;
        var current = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
        if (current != _lastState)
        {
            _lastState = current;
            _user = current.User;
        }
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
