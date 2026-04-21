namespace Menu.UI.Models.Restaurant;

public record RestaurantDto(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt);

public record CreateRestaurantRequest(
    string Name,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone,
    bool IsActive);

public record UpdateRestaurantRequest(
    string Name,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone,
    bool IsActive);
