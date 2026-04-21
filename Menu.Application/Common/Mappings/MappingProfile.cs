using AutoMapper;

using Menu.Application.DTOs.Category;
using Menu.Application.DTOs.ItemOption;
using Menu.Application.DTOs.Menu;
using Menu.Application.DTOs.MenuItem;
using Menu.Application.DTOs.OptionValue;
using Menu.Application.DTOs.Permission;
using Menu.Application.DTOs.Restaurant;
using Menu.Application.DTOs.Role;
using Menu.Application.DTOs.User;
using Menu.Domain.Entities;

namespace Menu.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Restaurant, RestaurantDto>();
        CreateMap<CreateRestaurantDto, Restaurant>();
        CreateMap<UpdateRestaurantDto, Restaurant>();

        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();

        CreateMap<Role, RoleDto>();
        CreateMap<CreateRoleDto, Role>();
        CreateMap<UpdateRoleDto, Role>();

        CreateMap<Permission, PermissionDto>();
        CreateMap<CreatePermissionDto, Permission>();
        CreateMap<UpdatePermissionDto, Permission>();

        CreateMap<Menu.Domain.Entities.Menu, MenuDto>();
        CreateMap<CreateMenuDto, Menu.Domain.Entities.Menu>();
        CreateMap<UpdateMenuDto, Menu.Domain.Entities.Menu>();

        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        CreateMap<MenuItem, MenuItemDto>();
        CreateMap<CreateMenuItemDto, MenuItem>();
        CreateMap<UpdateMenuItemDto, MenuItem>();

        CreateMap<ItemOption, ItemOptionDto>();
        CreateMap<CreateItemOptionDto, ItemOption>();
        CreateMap<UpdateItemOptionDto, ItemOption>();

        CreateMap<OptionValue, OptionValueDto>();
        CreateMap<CreateOptionValueDto, OptionValue>();
        CreateMap<UpdateOptionValueDto, OptionValue>();
    }
}
