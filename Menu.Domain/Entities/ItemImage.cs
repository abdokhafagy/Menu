
using System.ComponentModel.DataAnnotations;

namespace Menu.Domain.Entities;

public class ItemImage : BaseEntity
{
    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;


    // relationships
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
}
