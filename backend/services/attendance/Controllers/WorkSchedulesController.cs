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
[Route("api/v1/attendance/work-schedules")]
[Authorize]
public class WorkSchedulesController : ControllerBase
{
    private readonly IWorkScheduleService _scheduleService;

    public WorkSchedulesController(IWorkScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkScheduleDto>>>> GetSchedules(
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        var result = await _scheduleService.GetSchedulesAsync(employeeId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<WorkScheduleDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkScheduleDto>>> GetById(Guid id)
    {
        var result = await _scheduleService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<WorkScheduleDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<WorkScheduleDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkScheduleDto>>> Create([FromBody] CreateWorkScheduleDto dto)
    {
        var result = await _scheduleService.CreateAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<WorkScheduleDto>.Fail(result.Errors, result.Message));
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<WorkScheduleDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkScheduleDto>>> Update(Guid id, [FromBody] UpdateWorkScheduleDto dto)
    {
        var result = await _scheduleService.UpdateAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<WorkScheduleDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<WorkScheduleDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _scheduleService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }
}
