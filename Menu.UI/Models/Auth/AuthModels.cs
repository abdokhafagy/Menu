using Menu.UI.Models.User;

namespace Menu.UI.Models.Auth;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Device { get; set; }
}

public sealed class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public Guid RestaurantId { get; set; }
}

public record RefreshTokenRequest(string AccessToken, string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);

public record SessionInfo(Guid Id, string? Device, string? IpAddress, DateTime CreatedAt, DateTime ExpiresAt, bool IsRevoked);
