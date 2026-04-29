using Menu.Application.DTOs.Category;
using Menu.Application.DTOs.ItemOption;
using Menu.Application.DTOs.Menu;
using Menu.Application.DTOs.MenuItem;
using Menu.Application.DTOs.OptionValue;
using Menu.Application.DTOs.Permission;
using Menu.Application.DTOs.Restaurant;
using Menu.Application.DTOs.Role;
using Menu.Application.DTOs.User;
using System.Security.Claims;

namespace Menu.Application.Interfaces;

public interface IRestaurantService : ICrudService<RestaurantDto, CreateRestaurantDto, UpdateRestaurantDto>;
public interface IUserService : ICrudService<UserDto, CreateUserDto, UpdateUserDto>
{
    Task AssignRolesAsync(Guid userId, IReadOnlyList<Guid> roleIds, ClaimsPrincipal principal, CancellationToken ct = default);
}
public interface IRoleService : ICrudService<RoleDto, CreateRoleDto, UpdateRoleDto>
{
    Task AssignPermissionsAsync(Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken ct = default);
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(Guid roleId, CancellationToken ct = default);
}

public interface IPermissionService : ICrudService<PermissionDto, CreatePermissionDto, UpdatePermissionDto>;
public interface IMenuService : ICrudService<MenuDto, CreateMenuDto, UpdateMenuDto>;
public interface ICategoryService : ICrudService<CategoryDto, CreateCategoryDto, UpdateCategoryDto>;
public interface IMenuItemService : ICrudService<MenuItemDto, CreateMenuItemDto, UpdateMenuItemDto>;
public interface IItemOptionService : ICrudService<ItemOptionDto, CreateItemOptionDto, UpdateItemOptionDto>;
public interface IOptionValueService : ICrudService<OptionValueDto, CreateOptionValueDto, UpdateOptionValueDto>;
