namespace Menu.Application.DTOs.User;

public record CreateUserDto(
    string Username,
    string Email,
    string Password,
    string? FullName,
    bool IsActive,
    Guid RestaurantId);

public record UpdateUserDto(
    string Username,
    string Email,
    string? FullName,
    bool IsActive,
    Guid RestaurantId);
