using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Attendance.Application.Dtos;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Attendance.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hrms.Attendance.Controllers;

[ApiController]
[Route("api/v1/attendance/work-schedules")]
[Authorize]
public class WorkSchedulesController : ControllerBase
{
    private readonly IWorkScheduleService _scheduleService;
    private readonly AttendanceDbContext _dbContext;

    public WorkSchedulesController(IWorkScheduleService scheduleService, AttendanceDbContext dbContext)
    {
        _scheduleService = scheduleService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkScheduleDto>>>> GetSchedules(
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("PayrollStaff");
        var isManager = User.IsInRole("Manager");
        var employeeIdClaim = User.FindFirst("employeeId")?.Value;
        Guid.TryParse(employeeIdClaim, out var claimEmpId);

        if (!isSuperPrivileged)
        {
            if (isManager)
            {
                var managerEmp = await _dbContext.EmployeeProjections.FindAsync(claimEmpId);
                var managerDeptId = managerEmp?.DepartmentId;

                if (employeeId.HasValue)
                {
                    var targetEmp = await _dbContext.EmployeeProjections.FindAsync(employeeId.Value);
                    if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != claimEmpId))
                    {
                        return Forbid();
                    }
                }
                else
                {
                    var allowedEmpIds = await _dbContext.EmployeeProjections
                        .Where(e => e.DepartmentId == managerDeptId || e.ManagerEmployeeId == claimEmpId || e.Id == claimEmpId)
                        .Select(e => e.Id)
                        .ToListAsync();

                    var result = await _scheduleService.GetSchedulesAsync(null, fromDate, toDate, allowedEmpIds);
                    if (result.IsSuccess && result.Value != null)
                    {
                        return Ok(ApiResponse<IEnumerable<WorkScheduleDto>>.Ok(result.Value, result.Message));
                    }
                    return Ok(ApiResponse<IEnumerable<WorkScheduleDto>>.Ok(Array.Empty<WorkScheduleDto>(), result?.Message ?? "Success"));
                }
            }
            else // Employee
            {
                if (employeeId.HasValue && employeeId.Value != claimEmpId)
                {
                    return Forbid();
                }
                employeeId = claimEmpId;
            }
        }

        var resultNormal = await _scheduleService.GetSchedulesAsync(employeeId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<WorkScheduleDto>>.Ok(resultNormal.Value!, resultNormal.Message));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkScheduleDto>>> GetById(Guid id)
    {
        var result = await _scheduleService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<WorkScheduleDto>.Fail(result.Errors, result.Message));
        }

        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("PayrollStaff");
        var isManager = User.IsInRole("Manager");

        if (!isSuperPrivileged)
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (!Guid.TryParse(employeeIdClaim, out var claimEmpId))
            {
                return Forbid();
            }

            if (isManager)
            {
                var managerEmp = await _dbContext.EmployeeProjections.FindAsync(claimEmpId);
                var managerDeptId = managerEmp?.DepartmentId;

                var targetEmp = await _dbContext.EmployeeProjections.FindAsync(result.Value!.EmployeeId);
                if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != claimEmpId && targetEmp.Id != claimEmpId))
                {
                    return Forbid();
                }
            }
            else // Employee
            {
                if (claimEmpId != result.Value!.EmployeeId)
                {
                    return Forbid();
                }
            }
        }

        return Ok(ApiResponse<WorkScheduleDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<ApiResponse<WorkScheduleDto>>> Create([FromBody] CreateWorkScheduleDto dto)
    {
        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        if (!isSuperPrivileged && User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (!Guid.TryParse(employeeIdClaim, out var claimEmpId))
            {
                return Forbid();
            }

            var managerEmp = await _dbContext.EmployeeProjections.FindAsync(claimEmpId);
            var managerDeptId = managerEmp?.DepartmentId;

            var targetEmp = await _dbContext.EmployeeProjections.FindAsync(dto.EmployeeId);
            if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != claimEmpId))
            {
                return Forbid();
            }
        }

        var result = await _scheduleService.CreateAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<WorkScheduleDto>.Fail(result.Errors, result.Message));
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<WorkScheduleDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<ApiResponse<WorkScheduleDto>>> Update(Guid id, [FromBody] UpdateWorkScheduleDto dto)
    {
        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        if (!isSuperPrivileged && User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (!Guid.TryParse(employeeIdClaim, out var claimEmpId))
            {
                return Forbid();
            }

            var managerEmp = await _dbContext.EmployeeProjections.FindAsync(claimEmpId);
            var managerDeptId = managerEmp?.DepartmentId;

            var existingSchedule = await _scheduleService.GetByIdAsync(id);
            if (existingSchedule.IsFailure)
            {
                return NotFound(ApiResponse<WorkScheduleDto>.Fail(existingSchedule.Errors, existingSchedule.Message));
            }

            var targetEmp = await _dbContext.EmployeeProjections.FindAsync(existingSchedule.Value!.EmployeeId);
            if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != claimEmpId))
            {
                return Forbid();
            }
        }

        var result = await _scheduleService.UpdateAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<WorkScheduleDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<WorkScheduleDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        if (!isSuperPrivileged && User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (!Guid.TryParse(employeeIdClaim, out var claimEmpId))
            {
                return Forbid();
            }

            var managerEmp = await _dbContext.EmployeeProjections.FindAsync(claimEmpId);
            var managerDeptId = managerEmp?.DepartmentId;

            var existingSchedule = await _scheduleService.GetByIdAsync(id);
            if (existingSchedule.IsFailure)
            {
                return NotFound(ApiResponse.Fail(existingSchedule.Errors, existingSchedule.Message));
            }

            var targetEmp = await _dbContext.EmployeeProjections.FindAsync(existingSchedule.Value!.EmployeeId);
            if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != claimEmpId))
            {
                return Forbid();
            }
        }

        var result = await _scheduleService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }
}
