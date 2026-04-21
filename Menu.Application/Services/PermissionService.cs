using AutoMapper;

using Menu.Application.DTOs.Permission;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class PermissionService : CrudServiceBase<Permission, PermissionDto, CreatePermissionDto, UpdatePermissionDto>, IPermissionService
{
    public PermissionService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Permissions)
    {
    }
}
