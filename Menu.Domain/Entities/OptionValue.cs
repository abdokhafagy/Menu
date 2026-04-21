
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menu.Domain.Entities;

public class OptionValue : BaseEntity
{
    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ValueAr { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceModifier { get; set; } = 0;

    public bool IsDefault { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;


    // relationships
    public Guid ItemOptionId { get; set; }
    public ItemOption ItemOption { get; set; } = null!;
}
