namespace Menu.UI.Models.OptionValue;

public record OptionValueDto(
    Guid Id,
    string Value,
    string? ValueAr,
    decimal PriceModifier,
    bool IsDefault,
    int DisplayOrder,
    Guid ItemOptionId,
    DateTime CreatedAt,
    string? ItemOptionName = null);

public record CreateOptionValueRequest(
    string Value,
    string? ValueAr,
    decimal PriceModifier,
    bool IsDefault,
    int DisplayOrder,
    Guid ItemOptionId);

public record UpdateOptionValueRequest(
    string Value,
    string? ValueAr,
    decimal PriceModifier,
    bool IsDefault,
    int DisplayOrder);
