namespace Menu.UI.Models.Menu;

public record MenuDto(Guid Id, string Name, bool IsActive, Guid RestaurantId, DateTime CreatedAt, string? RestaurantName = null);

public record CreateMenuRequest(string Name, bool IsActive, Guid RestaurantId);

public record UpdateMenuRequest(string Name, bool IsActive);
