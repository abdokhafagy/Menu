namespace Menu.Application.DTOs.Permission;

public record PermissionDto(Guid Id, string Name, string? Description, string? Module, DateTime CreatedAt);
public record CreatePermissionDto(string Name, string? Description, string? Module);
public record UpdatePermissionDto(string Name, string? Description, string? Module);
public record AssignPermissionsDto(List<Guid> PermissionIds);
