
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menu.Domain.Entities;

public class MenuItem : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? DescriptionAr { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int DisplayOrder { get; set; } = 0;

    // relationships
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
    public ICollection<ItemOption> Options { get; set; } = new List<ItemOption>();
}
