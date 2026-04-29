namespace Menu.UI.Auth;

/// <summary>
/// Frontend mirror of the backend PermissionNames constants.
/// These strings must exactly match Menu.Domain.Authorization.PermissionNames.
/// Do NOT use raw string literals anywhere else — always reference this class.
/// </summary>
public static class AppPermissions
{
    // ── Restaurants ──────────────────────────────────────────────────────────
    public const string RestaurantsView   = "restaurants.view";
    public const string RestaurantsCreate = "restaurants.create";
    public const string RestaurantsUpdate = "restaurants.update";
    public const string RestaurantsDelete = "restaurants.delete";

    // ── Menus ─────────────────────────────────────────────────────────────────
    public const string MenusView   = "menus.view";
    public const string MenusCreate = "menus.create";
    public const string MenusUpdate = "menus.update";
    public const string MenusDelete = "menus.delete";

    // ── Categories ────────────────────────────────────────────────────────────
    public const string CategoriesView   = "categories.view";
    public const string CategoriesCreate = "categories.create";
    public const string CategoriesUpdate = "categories.update";
    public const string CategoriesDelete = "categories.delete";

    // ── Menu Items ────────────────────────────────────────────────────────────
    public const string MenuItemsView   = "menuitems.view";
    public const string MenuItemsCreate = "menuitems.create";
    public const string MenuItemsUpdate = "menuitems.update";
    public const string MenuItemsDelete = "menuitems.delete";

    // ── Item Options ──────────────────────────────────────────────────────────
    public const string ItemOptionsView   = "itemoptions.view";
    public const string ItemOptionsCreate = "itemoptions.create";
    public const string ItemOptionsUpdate = "itemoptions.update";
    public const string ItemOptionsDelete = "itemoptions.delete";

    // ── Option Values ─────────────────────────────────────────────────────────
    public const string OptionValuesView   = "optionvalues.view";
    public const string OptionValuesCreate = "optionvalues.create";
    public const string OptionValuesUpdate = "optionvalues.update";
    public const string OptionValuesDelete = "optionvalues.delete";

    // ── Permissions ───────────────────────────────────────────────────────────
    public const string PermissionsView   = "permissions.view";
    public const string PermissionsCreate = "permissions.create";
    public const string PermissionsUpdate = "permissions.update";
    public const string PermissionsDelete = "permissions.delete";

    // ── Roles ─────────────────────────────────────────────────────────────────
    public const string RolesView   = "roles.view";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";
    public const string RolesDelete = "roles.delete";

    // ── Users ─────────────────────────────────────────────────────────────────
    public const string UsersView   = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";

    // ── Sessions ──────────────────────────────────────────────────────────────
    public const string SessionsView   = "sessions.view";
    public const string SessionsCreate = "sessions.create";
    public const string SessionsUpdate = "sessions.update";
    public const string SessionsDelete = "sessions.delete";
}
