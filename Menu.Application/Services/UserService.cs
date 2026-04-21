using AutoMapper;
using Menu.Application.Common.Exceptions;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.User;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;

namespace Menu.Application.Services;

public class UserService : CrudServiceBase<Menu.Domain.Entities.User, UserDto, CreateUserDto, UpdateUserDto>, IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Users)
    {
        _unitOfWork = unitOfWork;
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
                restaurant.Name);

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
                restaurant.Name))
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

        var entity = new Menu.Domain.Entities.User
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            IsActive = dto.IsActive,
            RestaurantId = dto.RestaurantId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _unitOfWork.Users.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new UserDto(entity.Id, entity.Username, entity.Email, entity.FullName, entity.IsActive, entity.RestaurantId, entity.CreatedAt, null);
    }
}
