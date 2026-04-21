using Menu.UI.Models.Permission;

namespace Menu.UI.Services;

public sealed class PermissionService : CrudServiceBase<PermissionDto, CreatePermissionRequest, UpdatePermissionRequest>
{
    public PermissionService(IApiService api) : base(api, "api/permissions")
    {
    }
}
