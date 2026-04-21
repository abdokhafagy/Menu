
using System.ComponentModel.DataAnnotations;

namespace Menu.Domain.Entities;

public class Menu : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // relationships
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
