namespace Menu.UI.Models.User;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? FullName,
    bool IsActive,
    Guid RestaurantId,
    DateTime CreatedAt,
    string? RestaurantName = null);

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string? FullName,
    bool IsActive,
    Guid RestaurantId);

public record UpdateUserRequest(
    string Username,
    string Email,
    string? FullName,
    bool IsActive,
    Guid RestaurantId);
