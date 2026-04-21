using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Menu.Api.Filters;
using Menu.Application.Common.Models;
using Menu.Application.DTOs.OptionValue;
using Menu.Application.Interfaces;

namespace Menu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OptionValuesController : ControllerBase
{
    private readonly IOptionValueService _service;

    public OptionValuesController(IOptionValueService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<OptionValueDto>>>> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(ApiResponse<PaginatedResult<OptionValueDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OptionValueDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(ApiResponse<OptionValueDto>.SuccessResponse(result));
    }

    [HttpGet("/api/item-options/{optionId:guid}/values")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OptionValueDto>>>> GetByOption(Guid optionId, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 100 }, ct);
        var filtered = result.Data.Where(x => x.ItemOptionId == optionId).OrderBy(x => x.DisplayOrder).ToList();
        return Ok(ApiResponse<IReadOnlyList<OptionValueDto>>.SuccessResponse(filtered));
    }

    [HttpPost]
    [RequirePermission("optionvalues.create")]
    public async Task<ActionResult<ApiResponse<OptionValueDto>>> Create([FromBody] CreateOptionValueDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return Ok(ApiResponse<OptionValueDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("optionvalues.update")]
    public async Task<ActionResult<ApiResponse<OptionValueDto>>> Update(Guid id, [FromBody] UpdateOptionValueDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<OptionValueDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("optionvalues.delete")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<string>.SuccessResponse("Deleted"));
    }
}
