namespace Menu.Application.DTOs.Restaurant;

public record RestaurantDto(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt);

public record CreateRestaurantDto(
    string Name,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone,
    bool IsActive);

public record UpdateRestaurantDto(
    string Name,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone,
    bool IsActive);
