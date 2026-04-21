namespace Menu.Application.DTOs.Menu;

public record MenuDto(Guid Id, string Name, bool IsActive, Guid RestaurantId, DateTime CreatedAt, string? RestaurantName = null);
public record CreateMenuDto(string Name, bool IsActive, Guid RestaurantId);
public record UpdateMenuDto(string Name, bool IsActive);
