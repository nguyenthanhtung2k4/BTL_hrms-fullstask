using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.Attendance.Application.Dtos;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.Attendance.Controllers;

[ApiController]
[Route("api/v1/attendance/shifts")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ShiftDto>>>> GetAll()
    {
        var result = await _shiftService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ShiftDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> GetById(Guid id)
    {
        var result = await _shiftService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<ShiftDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<ShiftDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> Create([FromBody] CreateShiftDto dto)
    {
        var result = await _shiftService.CreateAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<ShiftDto>.Fail(result.Errors, result.Message));
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<ShiftDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> Update(Guid id, [FromBody] UpdateShiftDto dto)
    {
        var result = await _shiftService.UpdateAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<ShiftDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<ShiftDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _shiftService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }
}
