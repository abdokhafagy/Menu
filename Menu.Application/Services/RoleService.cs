using AutoMapper;

using Menu.Application.Common.Exceptions;
using Menu.Application.DTOs.Permission;
using Menu.Application.DTOs.Role;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class RoleService : CrudServiceBase<Role, RoleDto, CreateRoleDto, UpdateRoleDto>, IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Roles)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AssignPermissionsAsync(Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken ct = default)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId, ct);
        if (role is null)
        {
            throw new NotFoundException($"Role '{roleId}' was not found.");
        }

        var current = _unitOfWork.RolePermissions.Query().Where(x => x.RoleId == roleId).ToList();
        foreach (var item in current)
        {
            _unitOfWork.RolePermissions.HardDelete(item);
        }

        foreach (var permissionId in permissionIds.Distinct())
        {
            await _unitOfWork.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        var permissions =
            (from rp in _unitOfWork.RolePermissions.Query()
             join p in _unitOfWork.Permissions.Query() on rp.PermissionId equals p.Id
             where rp.RoleId == roleId
             select new PermissionDto(p.Id, p.Name, p.Description, p.Module, p.CreatedAt))
            .ToList();

        return Task.FromResult<IReadOnlyList<PermissionDto>>(permissions);
    }
}
