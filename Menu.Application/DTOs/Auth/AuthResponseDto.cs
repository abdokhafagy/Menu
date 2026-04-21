using Menu.Application.DTOs.User;

namespace Menu.Application.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);
