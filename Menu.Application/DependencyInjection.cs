using System.Reflection;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

using Menu.Application.Interfaces;
using Menu.Application.Services;

namespace Menu.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<IItemOptionService, ItemOptionService>();
        services.AddScoped<IOptionValueService, OptionValueService>();

        return services;
    }
}
