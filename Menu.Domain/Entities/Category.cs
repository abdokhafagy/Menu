
using System.ComponentModel.DataAnnotations;

namespace Menu.Domain.Entities;

public class Category : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    public int DisplayOrder { get; set; } = 0;

    // relationships
    public Guid MenuId { get; set; }
    public Menu Menu { get; set; } = null!;

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
