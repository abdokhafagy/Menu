namespace Menu.UI.Models.Public;

public record PublicRestaurantDto(
    Guid Id,
    string Name,
    string? Slug,
    string? Description,
    string? LogoUrl,
    string? Address,
    string? Phone);

public record PublicMenuDto(
    Guid Id,
    string Name);

public record PublicMenuSummaryDto(
    Guid Id,
    string Name,
    List<PublicCategoryMenuSummaryDto> Categories);

public record PublicCategoryMenuSummaryDto(
    Guid Id,
    string Name,
    string? NameAr,
    int DisplayOrder,
    List<PublicMenuItemSummaryDto> Items);

public record PublicMenuItemSummaryDto(
    Guid Id,
    string Name,
    string? NameAr,
    string? Description,
    string? DescriptionAr,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    bool IsPopular,
    bool IsNew,
    int DisplayOrder);

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

public record PublicOptionValueDto(
    Guid Id,
    string Value,
    string? ValueAr,
    decimal PriceModifier,
    bool IsDefault,
    int DisplayOrder);

public record PublicItemOptionDto(
    Guid Id,
    string Name,
    string? NameAr,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    string SelectionType,
    List<PublicOptionValueDto> Values);

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

public record PublicCategoryWithItemsDto(
    Guid Id,
    string Name,
    string? NameAr,
    int DisplayOrder,
    List<PublicMenuItemDto> Items);

public record PublicMenuFullDto(
    Guid Id,
    string Name,
    PublicRestaurantDto Restaurant,
    List<PublicCategoryWithItemsDto> Categories);
