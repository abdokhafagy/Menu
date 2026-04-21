namespace Menu.Application.DTOs.Role;

public record RoleDto(Guid Id, string Name, DateTime CreatedAt);
public record CreateRoleDto(string Name);
public record UpdateRoleDto(string Name);
