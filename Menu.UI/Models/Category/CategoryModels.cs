namespace Menu.UI.Models.Category;

public record CategoryDto(Guid Id, string Name, string? NameAr, int DisplayOrder, Guid MenuId, DateTime CreatedAt, string? MenuName = null);

public record CreateCategoryRequest(string Name, string? NameAr, int DisplayOrder, Guid MenuId);

public record UpdateCategoryRequest(string Name, string? NameAr, int DisplayOrder);
