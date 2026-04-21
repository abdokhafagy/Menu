
using System.ComponentModel.DataAnnotations;

namespace Menu.Domain.Entities;

public class Role : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;


    // relationships
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

