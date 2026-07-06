using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.HrCore.Controllers;

[ApiController]
[Route("api/v1/hr/positions")]
[Authorize]
public class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;

    public PositionsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PositionDto>>>> GetAll()
    {
        var result = await _positionService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PositionDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PositionDto>>> GetById(Guid id)
    {
        var result = await _positionService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<PositionDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<PositionDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<PositionDto>>> Create([FromBody] CreatePositionDto dto)
    {
        var result = await _positionService.CreateAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PositionDto>.Fail(result.Errors, result.Message));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<PositionDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<PositionDto>>> Update(Guid id, [FromBody] UpdatePositionDto dto)
    {
        var result = await _positionService.UpdateAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PositionDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<PositionDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _positionService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }
}
