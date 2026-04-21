namespace Menu.Application.DTOs.OptionValue;

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

public record CreateOptionValueDto(
    string Value,
    string? ValueAr,
    decimal PriceModifier,
    bool IsDefault,
    int DisplayOrder,
    Guid ItemOptionId);

public record UpdateOptionValueDto(
    string Value,
    string? ValueAr,
    decimal PriceModifier,
    bool IsDefault,
    int DisplayOrder);
