namespace Menu.UI.Models.ItemOption;

public enum SelectionType
{
    Single = 0,
    Multiple = 1
}

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

public record CreateItemOptionRequest(
    string Name,
    string? NameAr,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    SelectionType SelectionType,
    Guid MenuItemId);

public record UpdateItemOptionRequest(
    string Name,
    string? NameAr,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    SelectionType SelectionType);
