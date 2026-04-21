namespace Menu.UI.Models.Permission;

public record PermissionDto(Guid Id, string Name, string? Description, string? Module, DateTime CreatedAt);

public record CreatePermissionRequest(string Name, string? Description, string? Module);

public record UpdatePermissionRequest(string Name, string? Description, string? Module);

public record AssignPermissionsRequest(List<Guid> PermissionIds);
