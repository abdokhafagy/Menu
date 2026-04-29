using Menu.Domain.Entities;
using Menu.Domain.Authorization;
using Menu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Data.Seeding;

public static class DbSeeder
{
    private static readonly (string Name, string Module)[] RequiredPermissions =
    {
        (PermissionNames.RestaurantsView, "Restaurants"),
        (PermissionNames.RestaurantsCreate, "Restaurants"),
        (PermissionNames.RestaurantsUpdate, "Restaurants"),
        (PermissionNames.RestaurantsDelete, "Restaurants"),
        (PermissionNames.MenusView, "Menus"),
        (PermissionNames.MenusCreate, "Menus"),
        (PermissionNames.MenusUpdate, "Menus"),
        (PermissionNames.MenusDelete, "Menus"),
        (PermissionNames.CategoriesView, "Categories"),
        (PermissionNames.CategoriesCreate, "Categories"),
        (PermissionNames.CategoriesUpdate, "Categories"),
        (PermissionNames.CategoriesDelete, "Categories"),
        (PermissionNames.MenuItemsView, "MenuItems"),
        (PermissionNames.MenuItemsCreate, "MenuItems"),
        (PermissionNames.MenuItemsUpdate, "MenuItems"),
        (PermissionNames.MenuItemsDelete, "MenuItems"),
        (PermissionNames.ItemOptionsView, "ItemOptions"),
        (PermissionNames.ItemOptionsCreate, "ItemOptions"),
        (PermissionNames.ItemOptionsUpdate, "ItemOptions"),
        (PermissionNames.ItemOptionsDelete, "ItemOptions"),
        (PermissionNames.OptionValuesView, "OptionValues"),
        (PermissionNames.OptionValuesCreate, "OptionValues"),
        (PermissionNames.OptionValuesUpdate, "OptionValues"),
        (PermissionNames.OptionValuesDelete, "OptionValues"),
        (PermissionNames.PermissionsView, "Permissions"),
        (PermissionNames.PermissionsCreate, "Permissions"),
        (PermissionNames.PermissionsUpdate, "Permissions"),
        (PermissionNames.PermissionsDelete, "Permissions"),
        (PermissionNames.RolesView, "Roles"),
        (PermissionNames.RolesCreate, "Roles"),
        (PermissionNames.RolesUpdate, "Roles"),
        (PermissionNames.RolesDelete, "Roles"),
        (PermissionNames.UsersView, "Users"),
        (PermissionNames.UsersCreate, "Users"),
        (PermissionNames.UsersUpdate, "Users"),
        (PermissionNames.UsersDelete, "Users"),
        (PermissionNames.SessionsView, "Sessions"),
        (PermissionNames.SessionsCreate, "Sessions"),
        (PermissionNames.SessionsUpdate, "Sessions"),
        (PermissionNames.SessionsDelete, "Sessions")
    };

    private static readonly string[] SuperAdminPermissions = RequiredPermissions.Select(x => x.Name).ToArray();

    private static readonly string[] AdminPermissions =
    [
        PermissionNames.RolesView,
        PermissionNames.UsersView,
        PermissionNames.UsersCreate,
        PermissionNames.UsersUpdate,
        PermissionNames.UsersDelete,
        PermissionNames.MenusView,
        PermissionNames.MenusCreate,
        PermissionNames.MenusUpdate,
        PermissionNames.MenusDelete,
        PermissionNames.CategoriesView,
        PermissionNames.CategoriesCreate,
        PermissionNames.CategoriesUpdate,
        PermissionNames.CategoriesDelete,
        PermissionNames.MenuItemsView,
        PermissionNames.MenuItemsCreate,
        PermissionNames.MenuItemsUpdate,
        PermissionNames.MenuItemsDelete,
        PermissionNames.ItemOptionsView,
        PermissionNames.ItemOptionsCreate,
        PermissionNames.ItemOptionsUpdate,
        PermissionNames.ItemOptionsDelete,
        PermissionNames.OptionValuesView,
        PermissionNames.OptionValuesCreate,
        PermissionNames.OptionValuesUpdate,
        PermissionNames.OptionValuesDelete,
        PermissionNames.SessionsView,
        PermissionNames.SessionsCreate,
        PermissionNames.SessionsUpdate,
        PermissionNames.SessionsDelete
    ];

    private static readonly string[] ManagerPermissions =
    [
        PermissionNames.MenusView,
        PermissionNames.CategoriesView,
        PermissionNames.MenuItemsView,
        PermissionNames.MenuItemsCreate,
        PermissionNames.MenuItemsUpdate,
        PermissionNames.MenuItemsDelete,
        PermissionNames.ItemOptionsView,
        PermissionNames.ItemOptionsCreate,
        PermissionNames.ItemOptionsUpdate,
        PermissionNames.ItemOptionsDelete,
        PermissionNames.OptionValuesView,
        PermissionNames.OptionValuesCreate,
        PermissionNames.OptionValuesUpdate,
        PermissionNames.OptionValuesDelete
    ];

    public static async Task SeedAsync(AppDbContext context, CancellationToken ct = default)
    {
        var hadAnyRoles = await context.Roles.AnyAsync(ct);

        var existingPermissions = await context.Permissions
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var (name, module) in RequiredPermissions)
        {
            if (!existingPermissions.ContainsKey(name))
            {
                var permission = new Permission { Name = name, Module = module };
                await context.Permissions.AddAsync(permission, ct);
                existingPermissions[name] = permission;
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(ct);
        }

        var rolesByName = await context.Roles
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var roleName in new[] { RoleNames.SuperAdmin, RoleNames.Admin, RoleNames.Manager, RoleNames.User })
        {
            if (!rolesByName.ContainsKey(roleName))
            {
                var role = new Role { Name = roleName };
                await context.Roles.AddAsync(role, ct);
                rolesByName[roleName] = role;
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(ct);
        }

        var superAdminRole = rolesByName[RoleNames.SuperAdmin];
        var adminRole = rolesByName[RoleNames.Admin];
        var managerRole = rolesByName[RoleNames.Manager];
        var userRole = rolesByName[RoleNames.User];

        await EnsureRolePermissionsAsync(context, superAdminRole, SuperAdminPermissions, existingPermissions, ct);
        await EnsureRolePermissionsAsync(context, adminRole, AdminPermissions, existingPermissions, ct);
        await EnsureRolePermissionsAsync(context, managerRole, ManagerPermissions, existingPermissions, ct);
        await EnsureRolePermissionsAsync(context, userRole, Array.Empty<string>(), existingPermissions, ct);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(ct);
        }

        if (hadAnyRoles)
        {
            return;
        }

        var restaurant = new Restaurant
        {
            Name = "Demo Restaurant",
            Description = "Seeded demo restaurant",
            Address = "Main Street",
            Phone = "+1000000000",
            IsActive = true
        };

        await context.Restaurants.AddAsync(restaurant, ct);
        await context.SaveChangesAsync(ct);

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@menu.local",
            FullName = "System Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            RestaurantId = restaurant.Id,
            IsActive = true
        };

        await context.Users.AddAsync(adminUser, ct);
        await context.UserRoles.AddAsync(new UserRole { UserId = adminUser.Id, RoleId = superAdminRole.Id }, ct);

        var menu = new Menu.Domain.Entities.Menu { Name = "Main Menu", RestaurantId = restaurant.Id, IsActive = true };
        await context.Menus.AddAsync(menu, ct);

        var category = new Category { Name = "Burgers", MenuId = menu.Id, DisplayOrder = 1 };
        await context.Categories.AddAsync(category, ct);

        var item = new MenuItem
        {
            Name = "Classic Burger",
            Price = 9.99m,
            CategoryId = category.Id,
            IsAvailable = true,
            DisplayOrder = 1
        };
        await context.MenuItems.AddAsync(item, ct);

        var sizeOption = new ItemOption
        {
            Name = "Size",
            MenuItemId = item.Id,
            IsRequired = true,
            MinSelections = 1,
            MaxSelections = 1,
            SelectionType = Domain.Enums.SelectionType.Single
        };

        var toppingsOption = new ItemOption
        {
            Name = "Toppings",
            MenuItemId = item.Id,
            IsRequired = false,
            MinSelections = 0,
            MaxSelections = 5,
            SelectionType = Domain.Enums.SelectionType.Multiple
        };

        await context.ItemOptions.AddRangeAsync(new[] { sizeOption, toppingsOption }, ct);
        await context.SaveChangesAsync(ct);

        await context.OptionValues.AddRangeAsync(new[]
        {
            new OptionValue { Value = "Small", ItemOptionId = sizeOption.Id, PriceModifier = 0m },
            new OptionValue { Value = "Medium", ItemOptionId = sizeOption.Id, PriceModifier = 2m },
            new OptionValue { Value = "Large", ItemOptionId = sizeOption.Id, PriceModifier = 4m },
            new OptionValue { Value = "Cheese", ItemOptionId = toppingsOption.Id, PriceModifier = 1m },
            new OptionValue { Value = "Bacon", ItemOptionId = toppingsOption.Id, PriceModifier = 2m },
            new OptionValue { Value = "Jalapenos", ItemOptionId = toppingsOption.Id, PriceModifier = 0.5m }
        }, ct);

        await context.SaveChangesAsync(ct);
    }

    private static async Task EnsureRolePermissionsAsync(
        AppDbContext context,
        Role role,
        IReadOnlyCollection<string> permissionNames,
        IReadOnlyDictionary<string, Permission> permissionsByName,
        CancellationToken ct)
    {
        if (permissionNames.Count == 0)
        {
            return;
        }

        var existingPermissionIds = await context.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(ct);

        var missingRolePermissions = permissionNames
            .Where(permissionsByName.ContainsKey)
            .Select(name => permissionsByName[name])
            .Where(permission => !existingPermissionIds.Contains(permission.Id))
            .Select(permission => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            })
            .ToList();

        if (missingRolePermissions.Count > 0)
        {
            await context.RolePermissions.AddRangeAsync(missingRolePermissions, ct);
        }
    }
}
