using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(options, new NullTenantContext())
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Menu.Domain.Entities.Menu> Menus => Set<Menu.Domain.Entities.Menu>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<ItemOption> ItemOptions => Set<ItemOption>();
    public DbSet<OptionValue> OptionValues => Set<OptionValue>();
    public DbSet<ItemImage> ItemImages => Set<ItemImage>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplyTenantFilters(modelBuilder);
    }

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Restaurant>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.Id == _tenantContext.RestaurantId));

        modelBuilder.Entity<User>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.RestaurantId == _tenantContext.RestaurantId));

        modelBuilder.Entity<Menu.Domain.Entities.Menu>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.RestaurantId == _tenantContext.RestaurantId));

        modelBuilder.Entity<Category>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.Menu.RestaurantId == _tenantContext.RestaurantId));

        modelBuilder.Entity<MenuItem>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.Category.Menu.RestaurantId == _tenantContext.RestaurantId));

        modelBuilder.Entity<ItemOption>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.MenuItem.Category.Menu.RestaurantId == _tenantContext.RestaurantId));

        modelBuilder.Entity<OptionValue>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.ItemOption.MenuItem.Category.Menu.RestaurantId == _tenantContext.RestaurantId));

        modelBuilder.Entity<ItemImage>().HasQueryFilter(x =>
            !x.IsDeleted &&
            (!_tenantContext.RequiresRestaurantScope || x.MenuItem.Category.Menu.RestaurantId == _tenantContext.RestaurantId));
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private sealed class NullTenantContext : ITenantContext
    {
        public bool IsAuthenticated => false;
        public bool IsSuperAdmin => false;
        public bool RequiresRestaurantScope => false;
        public Guid? RestaurantId => null;

        public bool CanAccessRestaurant(Guid restaurantId) => true;

        public Guid GetRequiredRestaurantId() => throw new InvalidOperationException("Tenant context is not available.");
    }
}
