namespace MusicApp.DTOs;

public record RegisterDto(
    string Username,
    string Email,
    string Password,
    string Role // "Listener" or "Artist"
);

public record LoginDto(
    string Identifier, // email or username
    string Password
);

public record ResetPasswordDto(
    string Token,
    string NewPassword
);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
