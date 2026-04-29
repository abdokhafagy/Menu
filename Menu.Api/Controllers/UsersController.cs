using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.User;
using Menu.Application.Interfaces;
using Menu.Domain.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.Admin}")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUserService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.UsersView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<UserDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<UserDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.UsersView)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}/roles")]
    [Authorize(Policy = PermissionNames.UsersView)]
    public ActionResult<ApiResponse<IReadOnlyList<string>>> GetRoles(Guid id)
    {
        var roleIds = _unitOfWork.UserRoles.Query().Where(x => x.UserId == id).Select(x => x.RoleId).ToList();
        var roles = _unitOfWork.Roles.Query().Where(x => roleIds.Contains(x.Id)).Select(x => x.Name).ToList();
        return Ok(ApiResponse<IReadOnlyList<string>>.SuccessResponse(roles));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.UsersCreate)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.UsersUpdate)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.UsersDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }

    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = PermissionNames.UsersUpdate)]
    public async Task<ActionResult<ApiResponse<string>>> AssignRoles(Guid id, [FromBody] AssignRolesDto dto, CancellationToken ct)
    {
        await _service.AssignRolesAsync(id, dto.RoleIds, User, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Roles assigned."));
    }
}
