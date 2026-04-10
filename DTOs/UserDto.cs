// @ DTOs/UserDto.cs
using MusicApp.Enums;

namespace MusicApp.DTOs;

public record UserDto(
    int Id,
    string Username,
    string Email,
    Role Role,
    bool IsActive,
    DateTime CreatedAt
);
