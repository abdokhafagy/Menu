using Menu.UI.Auth;
using Menu.UI.State;

namespace Menu.UI.Navigation;

/// <summary>
/// Pure helpers that filter <see cref="AppNavigation.Items"/> by the current
/// user context. Centralized so the sidebar and bottom bar agree on visibility.
/// </summary>
public static class NavigationFilter
{
    public static IReadOnlyList<NavItem> Visible(UserContextService context, IReadOnlyList<NavItem>? items = null)
    {
        items ??= AppNavigation.Items;
        var result = new List<NavItem>(items.Count);
        foreach (var item in items)
        {
            if (IsVisible(item, context))
            {
                result.Add(item);
            }
        }
        return result;
    }

    public static IReadOnlyList<NavItem> VisibleForMobile(UserContextService context, int max = 5)
    {
        return Visible(context)
            .Where(i => i.ShowOnMobile)
            .OrderBy(i => i.MobilePriority)
            .Take(max)
            .ToList();
    }

    public static bool IsVisible(NavItem item, UserContextService context)
    {
        if (item.RequireAuthenticated && !context.IsAuthenticated())
        {
            return false;
        }

        // SuperAdmin bypasses permission/role gates and sees every nav entry.
        if (context.HasRole(AppRoles.SuperAdmin))
        {
            return true;
        }

        if (item.RequiredRoles.Length > 0 && !context.HasAnyRole(item.RequiredRoles))
        {
            return false;
        }

        if (item.RequiredPermissions.Length > 0 && !context.HasAllPermissions(item.RequiredPermissions))
        {
            return false;
        }

        return true;
    }
}
