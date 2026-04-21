using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Api.Filters;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.Permission;
using Menu.Application.DTOs.Role;
using Menu.Application.Interfaces;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<RoleDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<RoleDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(result));
    }

    [HttpPost]
    [RequirePermission("roles.create")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("roles.update")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, [FromBody] UpdateRoleDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("roles.delete")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }

    [HttpPost("{id:guid}/permissions")]
    [RequirePermission("roles.update")]
    public async Task<ActionResult<ApiResponse<string>>> AssignPermissions(Guid id, [FromBody] AssignPermissionsDto dto, CancellationToken ct)
    {
        await _service.AssignPermissionsAsync(id, dto.PermissionIds, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Permissions assigned."));
    }
}
