using System.Security.Claims;

using AutoMapper;
using Menu.Application.Common.Exceptions;
using Menu.Application.DTOs.Auth;
using Menu.Application.DTOs.User;
using Menu.Application.Interfaces;
using Menu.Domain.Entities;
using Menu.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Menu.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? ipAddress, CancellationToken ct = default)
    {
        var emailExists = await _unitOfWork.Users.ExistsAsync(x => x.Email == dto.Email, ct);
        if (emailExists)
        {
            throw new BadRequestException("Email is already registered.");
        }

        var usernameExists = await _unitOfWork.Users.ExistsAsync(x => x.Username == dto.Username, ct);
        if (usernameExists)
        {
            throw new BadRequestException("Username is already taken.");
        }

        if (!await _unitOfWork.Restaurants.ExistsAsync(x => x.Id == dto.RestaurantId, ct))
        {
            throw new NotFoundException($"Restaurant '{dto.RestaurantId}' was not found.");
        }

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RestaurantId = dto.RestaurantId,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, ct);

        var defaultRole = _unitOfWork.Roles.Query().FirstOrDefault(x => x.Name == "User");
        if (defaultRole is not null)
        {
            await _unitOfWork.UserRoles.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return await CreateAuthResponseAsync(user, null, ipAddress, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress, CancellationToken ct = default)
    {
        var user = _unitOfWork.Users.Query().FirstOrDefault(x => x.Email == dto.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return await CreateAuthResponseAsync(user, dto.Device, ipAddress, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, string? ipAddress, CancellationToken ct = default)
    {
        var principal = _tokenService.ValidateExpiredToken(dto.AccessToken);
        if (principal is null)
        {
            throw new UnauthorizedException("Invalid access token.");
        }

        var sid = principal.FindFirst("sid")?.Value;
        var jti = principal.FindFirst(ClaimTypes.SerialNumber)?.Value ?? principal.FindFirst("jti")?.Value;

        if (!Guid.TryParse(sid, out var sessionId))
        {
            throw new UnauthorizedException("Session claim is missing.");
        }

        var session = _unitOfWork.UserSessions.Query().FirstOrDefault(x => x.Id == sessionId);
        if (session is null || session.IsRevoked || session.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Session is no longer valid.");
        }

        if (!string.Equals(session.Jti, jti, StringComparison.Ordinal))
        {
            throw new UnauthorizedException("Token does not match session state.");
        }

        var incomingHash = _tokenService.HashToken(dto.RefreshToken);
        if (!string.Equals(incomingHash, session.RefreshTokenHash, StringComparison.Ordinal))
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        _unitOfWork.UserSessions.Update(session);

        var user = _unitOfWork.Users.Query().FirstOrDefault(x => x.Id == session.UserId);
        if (user is null)
        {
            throw new UnauthorizedException("User not found.");
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return await CreateAuthResponseAsync(user, session.Device, ipAddress, ct);
    }

    public async Task LogoutAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var sessionId = GetSessionId(principal);
        var session = _unitOfWork.UserSessions.Query().FirstOrDefault(x => x.Id == sessionId);
        if (session is null)
        {
            return;
        }

        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        _unitOfWork.UserSessions.Update(session);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task LogoutAllAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var userId = GetUserId(principal);
        var sessions = _unitOfWork.UserSessions.Query()
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToList();

        foreach (var session in sessions)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            _unitOfWork.UserSessions.Update(session);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public Task<IReadOnlyList<SessionDto>> GetSessionsAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var userId = GetUserId(principal);
        var sessions = _unitOfWork.UserSessions.Query()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SessionDto(x.Id, x.Device, x.IpAddress, x.CreatedAt, x.RefreshTokenExpiresAt, x.IsRevoked))
            .ToList();

        return Task.FromResult<IReadOnlyList<SessionDto>>(sessions);
    }

    public async Task RevokeSessionAsync(ClaimsPrincipal principal, Guid sessionId, CancellationToken ct = default)
    {
        var userId = GetUserId(principal);
        var session = _unitOfWork.UserSessions.Query().FirstOrDefault(x => x.Id == sessionId);
        if (session is null)
        {
            throw new NotFoundException($"Session '{sessionId}' was not found.");
        }

        if (session.UserId != userId)
        {
            throw new ForbiddenException("You cannot revoke another user's session.");
        }

        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;
        _unitOfWork.UserSessions.Update(session);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(User user, string? device, string? ipAddress, CancellationToken ct)
    {
        var roleIds = _unitOfWork.UserRoles.Query()
            .Where(x => x.UserId == user.Id)
            .Select(x => x.RoleId)
            .ToList();

        var roles = _unitOfWork.Roles.Query()
            .Where(x => roleIds.Contains(x.Id))
            .Select(x => x.Name)
            .Distinct()
            .ToList();

        var permissions = (
            from rolePermission in _unitOfWork.RolePermissions.Query()
            join permission in _unitOfWork.Permissions.Query() on rolePermission.PermissionId equals permission.Id
            where roleIds.Contains(rolePermission.RoleId)
            select permission.Name)
            .Distinct()
            .ToList();

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(refreshToken);

        var refreshDaysText = _configuration["Jwt:RefreshTokenExpirationDays"];
        var refreshDays = int.TryParse(refreshDaysText, out var parsedRefreshDays) ? parsedRefreshDays : 7;
        var session = new UserSession
        {
            UserId = user.Id,
            Device = device,
            IpAddress = ipAddress,
            RefreshTokenHash = refreshTokenHash,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
        };

        var (accessToken, jti, expiresAt) = await _tokenService.GenerateAccessTokenAsync(
            user,
            session.Id,
            roles,
            permissions,
            ct);

        session.Jti = jti;
        session.AccessTokenExpiresAt = expiresAt;

        await _unitOfWork.UserSessions.AddAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResponseDto(accessToken, refreshToken, expiresAt, _mapper.Map<UserDto>(user));
    }

    private static Guid GetUserId(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedException("User claim is missing.");
        }

        return parsedUserId;
    }

    private static Guid GetSessionId(ClaimsPrincipal principal)
    {
        var sid = principal.FindFirst("sid")?.Value;
        if (!Guid.TryParse(sid, out var parsedSid))
        {
            throw new UnauthorizedException("Session claim is missing.");
        }

        return parsedSid;
    }
}
