
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Menu.Domain.Entities;

public class User : BaseEntity
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? FullName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }


    // relationships
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
