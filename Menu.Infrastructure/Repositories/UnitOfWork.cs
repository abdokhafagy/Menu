using Menu.Domain.Entities;
using Menu.Domain.Interfaces;
using Menu.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Menu.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IGenericRepository<Restaurant>? _restaurants;
    private IGenericRepository<User>? _users;
    private IGenericRepository<Role>? _roles;
    private IGenericRepository<UserRole>? _userRoles;
    private IGenericRepository<Permission>? _permissions;
    private IGenericRepository<RolePermission>? _rolePermissions;
    private IGenericRepository<Menu.Domain.Entities.Menu>? _menus;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<MenuItem>? _menuItems;
    private IGenericRepository<ItemOption>? _itemOptions;
    private IGenericRepository<OptionValue>? _optionValues;
    private IGenericRepository<ItemImage>? _itemImages;
    private IGenericRepository<UserSession>? _userSessions;

    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<Restaurant> Restaurants => _restaurants ??= new GenericRepository<Restaurant>(_context);
    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<Role> Roles => _roles ??= new GenericRepository<Role>(_context);
    public IGenericRepository<UserRole> UserRoles => _userRoles ??= new GenericRepository<UserRole>(_context);
    public IGenericRepository<Permission> Permissions => _permissions ??= new GenericRepository<Permission>(_context);
    public IGenericRepository<RolePermission> RolePermissions => _rolePermissions ??= new GenericRepository<RolePermission>(_context);
    public IGenericRepository<Menu.Domain.Entities.Menu> Menus => _menus ??= new GenericRepository<Menu.Domain.Entities.Menu>(_context);
    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
    public IGenericRepository<MenuItem> MenuItems => _menuItems ??= new GenericRepository<MenuItem>(_context);
    public IGenericRepository<ItemOption> ItemOptions => _itemOptions ??= new GenericRepository<ItemOption>(_context);
    public IGenericRepository<OptionValue> OptionValues => _optionValues ??= new GenericRepository<OptionValue>(_context);
    public IGenericRepository<ItemImage> ItemImages => _itemImages ??= new GenericRepository<ItemImage>(_context);
    public IGenericRepository<UserSession> UserSessions => _userSessions ??= new GenericRepository<UserSession>(_context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
        {
            _transaction = await _context.Database.BeginTransactionAsync(ct);
        }
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
