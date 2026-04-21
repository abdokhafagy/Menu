namespace Menu.Application.DTOs.MenuItem;

public record MenuItemDto(
    Guid Id,
    string Name,
    string? NameAr,
    string? Description,
    string? DescriptionAr,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int DisplayOrder,
    Guid CategoryId,
    DateTime CreatedAt,
    string? CategoryName = null);

public record CreateMenuItemDto(
    string Name,
    string? NameAr,
    string? Description,
    string? DescriptionAr,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int DisplayOrder,
    Guid CategoryId);

public record UpdateMenuItemDto(
    string Name,
    string? NameAr,
    string? Description,
    string? DescriptionAr,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int DisplayOrder);
