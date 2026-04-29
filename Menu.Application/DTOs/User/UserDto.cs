namespace Menu.Application.DTOs.User;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? FullName,
    bool IsActive,
    Guid RestaurantId,
    DateTime CreatedAt,
    string? RestaurantName = null,
    IReadOnlyList<string>? Roles = null);
