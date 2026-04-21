
using System.ComponentModel.DataAnnotations;

namespace Menu.Domain.Entities;

public class Restaurant : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Slug { get; set; } // URL-friendly identifier e.g. "ch-culinary"

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    //[MaxLength(3)]
    //public string Currency { get; set; } = "EGP";

    public bool IsActive { get; set; } = true;

    // relationships
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
