namespace Menu.UI.Models.Role;

public record RoleDto(Guid Id, string Name, DateTime CreatedAt);

public record CreateRoleRequest(string Name);

public record UpdateRoleRequest(string Name);
