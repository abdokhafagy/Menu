
using System.ComponentModel.DataAnnotations;

using Menu.Domain.Enums;

namespace Menu.Domain.Entities;

public class ItemOption : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    public bool IsRequired { get; set; } = false;

    public int MinSelections { get; set; } = 0;

    public int MaxSelections { get; set; } = 1;

    public SelectionType SelectionType { get; set; } = SelectionType.Single;


    // relationships
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public ICollection<OptionValue> Values { get; set; } = new List<OptionValue>();
}
