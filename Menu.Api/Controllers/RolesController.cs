using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Application.Common.Models;
using Menu.Application.DTOs.Permission;
using Menu.Application.DTOs.Role;
using Menu.Application.Interfaces;
using Menu.Domain.Authorization;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.Admin}")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.RolesView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<RoleDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<RoleDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionNames.RolesView)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.RolesCreate)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionNames.RolesUpdate)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, [FromBody] UpdateRoleDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionNames.RolesDelete)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }

    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = PermissionNames.RolesView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PermissionDto>>>> GetPermissions(Guid id, CancellationToken ct)
    {
        var result = await _service.GetPermissionsAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<PermissionDto>>.SuccessResponse(result));
    }

    [HttpPost("{id:guid}/permissions")]
    [Authorize(Policy = PermissionNames.RolesUpdate)]
    public async Task<ActionResult<ApiResponse<string>>> AssignPermissions(Guid id, [FromBody] AssignPermissionsDto dto, CancellationToken ct)
    {
        await _service.AssignPermissionsAsync(id, dto.PermissionIds, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Permissions assigned."));
    }
}
