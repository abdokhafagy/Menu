namespace Menu.Application.DTOs.Auth;

public record SessionDto(
    Guid Id,
    string? Device,
    string? IpAddress,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsRevoked);
