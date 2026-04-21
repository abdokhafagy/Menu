using Menu.Domain.Entities;

namespace Menu.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Restaurant> Restaurants { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<Role> Roles { get; }
    IGenericRepository<UserRole> UserRoles { get; }
    IGenericRepository<Permission> Permissions { get; }
    IGenericRepository<RolePermission> RolePermissions { get; }
    IGenericRepository<Menu.Domain.Entities.Menu> Menus { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<MenuItem> MenuItems { get; }
    IGenericRepository<ItemOption> ItemOptions { get; }
    IGenericRepository<OptionValue> OptionValues { get; }
    IGenericRepository<ItemImage> ItemImages { get; }
    IGenericRepository<UserSession> UserSessions { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}