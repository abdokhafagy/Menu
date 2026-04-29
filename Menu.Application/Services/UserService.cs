using AutoMapper;
using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.User;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;
using System.Security.Claims;

namespace Menu.Application.Services;

public class UserService : CrudServiceBase<Menu.Domain.Entities.User, UserDto, CreateUserDto, UpdateUserDto>, IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
        : base(unitOfWork, mapper, unitOfWork.Users)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public override Task<PaginatedResult<UserDto>> GetAllAsync(QueryParameters parameters, CancellationToken ct = default)
    {
        var query =
            from user in Repository.Query()
            join restaurant in UnitOfWork.Restaurants.Query() on user.RestaurantId equals restaurant.Id
            select new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                user.IsActive,
                user.RestaurantId,
                user.CreatedAt,
                restaurant.Name,
                user.UserRoles.Select(ur => ur.Role.Name).ToList());

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Username.ToLower().Contains(search) ||
                x.Email.ToLower().Contains(search) ||
                (x.FullName ?? string.Empty).ToLower().Contains(search) ||
                (x.RestaurantName ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = query.Count();
        var data = query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResult<UserDto>
        {
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            Data = data
        });
    }

    public override Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.User)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        var dto = (
            from user in Repository.Query()
            join restaurant in UnitOfWork.Restaurants.Query() on user.RestaurantId equals restaurant.Id
            where user.Id == id
            select new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                user.IsActive,
                user.RestaurantId,
                user.CreatedAt,
                restaurant.Name,
                user.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .FirstOrDefault();

        if (dto is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.User)} '{id}' was not found.");
        }

        return Task.FromResult(dto);
    }

    public override async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var emailExists = await _unitOfWork.Users.ExistsAsync(x => x.Email == dto.Email, ct);
        if (emailExists)
        {
            throw new BadRequestException("Email is already registered.");
        }

        var restaurantId = _tenantContext.RequiresRestaurantScope ? _tenantContext.GetRequiredRestaurantId() : dto.RestaurantId;
        if (restaurantId == Guid.Empty)
        {
            throw new BadRequestException("Restaurant is required.");
        }

        var restaurantExists = await _unitOfWork.Restaurants.ExistsAsync(x => x.Id == restaurantId, ct);
        if (!restaurantExists)
        {
            throw new NotFoundException($"Restaurant '{restaurantId}' was not found.");
        }

        var entity = new Menu.Domain.Entities.User
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            IsActive = dto.IsActive,
            RestaurantId = restaurantId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _unitOfWork.Users.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new UserDto(entity.Id, entity.Username, entity.Email, entity.FullName, entity.IsActive, entity.RestaurantId, entity.CreatedAt, null, Array.Empty<string>());
    }

    public override async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.User)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        entity.Username = dto.Username;
        entity.Email = dto.Email;
        entity.FullName = dto.FullName;
        entity.IsActive = dto.IsActive;

        Repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public override async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException($"{nameof(Menu.Domain.Entities.User)} '{id}' was not found.");
        }

        EnsureRestaurantAccess(entity.RestaurantId);

        Repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task AssignRolesAsync(Guid userId, IReadOnlyList<Guid> roleIds, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var targetUser = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == userId);
        if (targetUser is null)
        {
            throw new NotFoundException($"{nameof(User)} '{userId}' was not found.");
        }

        EnsureRestaurantAccess(targetUser.RestaurantId);

        var currentUserId = GetCurrentUserId(principal);
        var currentUser = Repository.Query(ignoreQueryFilters: true).FirstOrDefault(x => x.Id == currentUserId);
        if (currentUser is null)
        {
            throw new UnauthorizedException("Current user was not found.");
        }

        var currentRoleNames = await GetRoleNamesAsync(currentUserId, ct);
        var isSuperAdmin = currentRoleNames.Any(x => x.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
        var isAdmin = currentRoleNames.Any(x => x.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        if (!isSuperAdmin && !isAdmin)
        {
            throw new ForbiddenException("You do not have permission to assign roles.");
        }

        if (isAdmin && !isSuperAdmin)
        {
            if (currentUser.RestaurantId != targetUser.RestaurantId)
            {
                throw new ForbiddenException("You can only manage users within your restaurant.");
            }

            var requestedRoleNames = await GetRoleNamesByIdsAsync(roleIds, ct);
            var forbiddenRole = requestedRoleNames.FirstOrDefault(role =>
                !role.Equals(RoleNames.Manager, StringComparison.OrdinalIgnoreCase) &&
                !role.Equals(RoleNames.User, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(forbiddenRole))
            {
                throw new ForbiddenException("Admin can only assign Manager or User roles.");
            }
        }

        var currentAssignments = _unitOfWork.UserRoles.Query().Where(x => x.UserId == userId).ToList();
        foreach (var assignment in currentAssignments)
        {
            _unitOfWork.UserRoles.HardDelete(assignment);
        }

        foreach (var roleId in roleIds.Distinct())
        {
            var roleExists = await _unitOfWork.Roles.ExistsAsync(x => x.Id == roleId, ct);
            if (!roleExists)
            {
                throw new NotFoundException($"Role '{roleId}' was not found.");
            }

            await _unitOfWork.UserRoles.AddAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private Guid GetCurrentUserId(ClaimsPrincipal principal)
    {
        var rawUserId = principal.FindFirst(JwtClaimTypes.UserId)?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(rawUserId, out var currentUserId))
        {
            throw new UnauthorizedException("User identifier is missing.");
        }

        return currentUserId;
    }

    private Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct)
    {
        var roleIds = _unitOfWork.UserRoles.Query()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToList();

        var roleNames = _unitOfWork.Roles.Query()
            .Where(x => roleIds.Contains(x.Id))
            .Select(x => x.Name)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(roleNames);
    }

    private Task<IReadOnlyList<string>> GetRoleNamesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken ct)
    {
        var ids = roleIds.Distinct().ToList();

        var roleNames = _unitOfWork.Roles.Query()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Name)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(roleNames);
    }

    private void EnsureRestaurantAccess(Guid restaurantId)
    {
        if (!_tenantContext.CanAccessRestaurant(restaurantId))
        {
            throw new ForbiddenException("You do not have access to this restaurant.");
        }
    }
}
