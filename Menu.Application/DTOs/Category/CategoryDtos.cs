namespace Menu.Application.DTOs.Category;

public record CategoryDto(Guid Id, string Name, string? NameAr, int DisplayOrder, Guid MenuId, DateTime CreatedAt, string? MenuName = null);
public record CreateCategoryDto(string Name, string? NameAr, int DisplayOrder, Guid MenuId);
public record UpdateCategoryDto(string Name, string? NameAr, int DisplayOrder);
