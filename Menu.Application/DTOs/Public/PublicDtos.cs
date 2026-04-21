namespace Menu.Application.DTOs.Public;

// ═══ Restaurant ═══
public record PublicRestaurantDto(
    Guid Id,
    string Name,
    string? Slug,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone);

// ═══ Menu (lightweight list) ═══
public record PublicMenuDto(
    Guid Id,
    string Name);

// ═══ Category ═══
public record PublicCategoryDto(
    Guid Id,
    string Name,
    string? NameAr,
    int DisplayOrder);

// ═══ Menu Item (card view) ═══
public record PublicMenuItemDto(
    Guid Id,
    string Name,
    string? NameAr,
    string? Description,
    string? DescriptionAr,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int DisplayOrder,
    List<PublicItemOptionDto>? Options = null);

// ═══ Item Option ═══
public record PublicItemOptionDto(
    Guid Id,
    string Name,
    string? NameAr,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    string SelectionType,
    List<PublicOptionValueDto> Values);

// ═══ Option Value ═══
public record PublicOptionValueDto(
    Guid Id,
    string Value,
    string? ValueAr,
    decimal PriceModifier,
    bool IsDefault,
    int DisplayOrder);

// ═══ Item Detail (full with options) ═══
public record PublicMenuItemDetailDto(
    Guid Id,
    string Name,
    string? NameAr,
    string? Description,
    string? DescriptionAr,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    List<PublicItemOptionDto> Options,
    List<string> Images);

// ═══ Category with Items (for menu tree) ═══
public record PublicCategoryWithItemsDto(
    Guid Id,
    string Name,
    string? NameAr,
    int DisplayOrder,
    List<PublicMenuItemDto> Items);

// ═══ Full Menu Tree (single request loads everything) ═══
public record PublicMenuFullDto(
    Guid Id,
    string Name,
    PublicRestaurantDto Restaurant,
    List<PublicCategoryWithItemsDto> Categories);
