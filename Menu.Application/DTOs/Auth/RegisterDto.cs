namespace Menu.Application.DTOs.Auth;

public record RegisterDto(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword,
    string? FullName,
    Guid RestaurantId);
