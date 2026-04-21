using Menu.Domain.Entities;
using Menu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Data.Seeding;

public static class DbSeeder
{
    private static readonly (string Name, string Module)[] RequiredPermissions =
    {
        ("restaurants.view", "Restaurants"),
        ("restaurants.create", "Restaurants"),
        ("restaurants.update", "Restaurants"),
        ("restaurants.delete", "Restaurants"),
        ("menus.view", "Menus"),
        ("menus.create", "Menus"),
        ("menus.update", "Menus"),
        ("menus.delete", "Menus"),
        ("categories.view", "Categories"),
        ("categories.create", "Categories"),
        ("categories.update", "Categories"),
        ("categories.delete", "Categories"),
        ("menuitems.view", "MenuItems"),
        ("menuitems.create", "MenuItems"),
        ("menuitems.update", "MenuItems"),
        ("menuitems.delete", "MenuItems"),
        ("itemoptions.view", "ItemOptions"),
        ("itemoptions.create", "ItemOptions"),
        ("itemoptions.update", "ItemOptions"),
        ("itemoptions.delete", "ItemOptions"),
        ("optionvalues.view", "OptionValues"),
        ("optionvalues.create", "OptionValues"),
        ("optionvalues.update", "OptionValues"),
        ("optionvalues.delete", "OptionValues"),
        ("permissions.view", "Permissions"),
        ("permissions.create", "Permissions"),
        ("permissions.update", "Permissions"),
        ("permissions.delete", "Permissions"),
        ("roles.view", "Roles"),
        ("roles.create", "Roles"),
        ("roles.update", "Roles"),
        ("roles.delete", "Roles"),
        ("users.view", "Users"),
        ("users.create", "Users"),
        ("users.update", "Users"),
        ("users.delete", "Users")
    };

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

        foreach (var roleName in new[] { "SuperAdmin", "Admin", "Manager", "User" })
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

        var superAdminRole = rolesByName["SuperAdmin"];

        var existingSuperAdminPermissionIds = await context.RolePermissions
            .Where(x => x.RoleId == superAdminRole.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(ct);

        var missingSuperAdminRolePermissions = existingPermissions.Values
            .Where(permission => !existingSuperAdminPermissionIds.Contains(permission.Id))
            .Select(permission => new RolePermission
            {
                RoleId = superAdminRole.Id,
                PermissionId = permission.Id
            })
            .ToList();

        if (missingSuperAdminRolePermissions.Count > 0)
        {
            await context.RolePermissions.AddRangeAsync(missingSuperAdminRolePermissions, ct);
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
}
