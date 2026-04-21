using Menu.Domain.Enums;

namespace Menu.Application.DTOs.ItemOption;

public record ItemOptionDto(
    Guid Id,
    string Name,
    string? NameAr,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    SelectionType SelectionType,
    Guid MenuItemId,
    DateTime CreatedAt,
    string? MenuItemName = null);

public record CreateItemOptionDto(
    string Name,
    string? NameAr,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    SelectionType SelectionType,
    Guid MenuItemId);

public record UpdateItemOptionDto(
    string Name,
    string? NameAr,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    SelectionType SelectionType);
