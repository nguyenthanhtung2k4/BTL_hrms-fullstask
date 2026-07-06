using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hrms.Attendance.Application.Dtos;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Hrms.Attendance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hrms.Attendance.Controllers;

[ApiController]
[Route("api/v1/attendance/timesheets")]
[Authorize]
public class TimesheetsController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;
    private readonly AttendanceDbContext _dbContext;

    public TimesheetsController(ITimesheetService timesheetService, AttendanceDbContext dbContext)
    {
        _timesheetService = timesheetService;
        _dbContext = dbContext;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TimesheetDto>>>> GetMyTimesheets([FromQuery] int? year)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<IEnumerable<TimesheetDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _timesheetService.GetPersonalTimesheetsAsync(employeeId, year);
        return Ok(ApiResponse<IEnumerable<TimesheetDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TimesheetDto>>>> GetTimesheets(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? employeeId)
    {
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("PayrollStaff");
        var isManager = User.IsInRole("Manager");

        if (!isPrivileged)
        {
            if (isManager)
            {
                var currentEmpId = GetCurrentEmployeeId();
                if (currentEmpId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<IEnumerable<TimesheetDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
                }

                var managerEmp = await _dbContext.EmployeeProjections.FindAsync(currentEmpId);
                var managerDeptId = managerEmp?.DepartmentId;

                if (departmentId.HasValue && departmentId.Value != managerDeptId)
                {
                    return Forbid();
                }
                departmentId = managerDeptId;

                if (employeeId.HasValue)
                {
                    var targetEmp = await _dbContext.EmployeeProjections.FindAsync(employeeId.Value);
                    if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != currentEmpId))
                    {
                        return Forbid();
                    }
                }
            }
            else
            {
                var currentEmpId = GetCurrentEmployeeId();
                if (currentEmpId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<IEnumerable<TimesheetDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
                }

                // Standard Employee is restricted to their own records only
                employeeId = currentEmpId;
                departmentId = null;
            }
        }

        var result = await _timesheetService.GetTimesheetsAsync(year, month, departmentId, employeeId);
        return Ok(ApiResponse<IEnumerable<TimesheetDto>>.Ok(result.Value!, result.Message));
    }

    [HttpPost("recalculate")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse>> Recalculate(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var result = await _timesheetService.RecalculateTimesheetsAsync(year, month);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }

    private Guid GetCurrentEmployeeId()
    {
        var claimValue = User.FindFirst("employeeId")?.Value;
        if (Guid.TryParse(claimValue, out var employeeId))
        {
            return employeeId;
        }
        return Guid.Empty;
    }
}
