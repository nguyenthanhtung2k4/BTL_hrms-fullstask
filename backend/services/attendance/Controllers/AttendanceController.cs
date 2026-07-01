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
[Route("api/v1/attendance/records")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly AttendanceDbContext _dbContext;

    public AttendanceController(IAttendanceService attendanceService, AttendanceDbContext dbContext)
    {
        _attendanceService = attendanceService;
        _dbContext = dbContext;
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<ApiResponse<AttendanceRecordDto>>> CheckIn([FromBody] CheckInRequest request)
    {
        if (User.IsInRole("Admin"))
        {
            return BadRequest(ApiResponse<AttendanceRecordDto>.Fail("AdminForbidden", "Administrators are not permitted to perform check-in."));
        }

        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<AttendanceRecordDto>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _attendanceService.CheckInAsync(employeeId, request.ShiftCode);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<AttendanceRecordDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<AttendanceRecordDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost("check-out")]
    public async Task<ActionResult<ApiResponse<AttendanceRecordDto>>> CheckOut()
    {
        if (User.IsInRole("Admin"))
        {
            return BadRequest(ApiResponse<AttendanceRecordDto>.Fail("AdminForbidden", "Administrators are not permitted to perform check-out."));
        }

        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<AttendanceRecordDto>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _attendanceService.CheckOutAsync(employeeId);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<AttendanceRecordDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<AttendanceRecordDto>.Ok(result.Value!, result.Message));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceRecordDto>>>> GetMyRecords(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        if (User.IsInRole("Admin"))
        {
            return BadRequest(ApiResponse<IEnumerable<AttendanceRecordDto>>.Fail("AdminForbidden", "Administrators do not have personal attendance records."));
        }

        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<IEnumerable<AttendanceRecordDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _attendanceService.GetPersonalRecordsAsync(employeeId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<AttendanceRecordDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceRecordDto>>>> GetRecords(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("PayrollStaff");
        var isManager = User.IsInRole("Manager");

        if (!isPrivileged)
        {
            var currentEmpId = GetCurrentEmployeeId();
            if (currentEmpId == Guid.Empty)
            {
                return BadRequest(ApiResponse<IEnumerable<AttendanceRecordDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
            }

            if (isManager)
            {
                // Retrieve Manager's department
                var managerEmp = await _dbContext.EmployeeProjections.FindAsync(currentEmpId);
                var managerDeptId = managerEmp?.DepartmentId;

                // Enforce that manager can only view their own department
                if (departmentId.HasValue && departmentId.Value != managerDeptId)
                {
                    return Forbid();
                }
                departmentId = managerDeptId;

                // Enforce target employee belongs to manager's department or reports directly
                if (employeeId.HasValue)
                {
                    var targetEmp = await _dbContext.EmployeeProjections.FindAsync(employeeId.Value);
                    if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != currentEmpId))
                    {
                        return Forbid();
                    }
                }
            }
            else // Standard Employee is restricted to their own records only
            {
                employeeId = currentEmpId;
                departmentId = null;
            }
        }

        if (month.HasValue && year.HasValue)
        {
            fromDate = new DateOnly(year.Value, month.Value, 1);
            toDate = fromDate.Value.AddMonths(1).AddDays(-1);
        }

        var result = await _attendanceService.GetRecordsAsync(employeeId, departmentId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<AttendanceRecordDto>>.Ok(result.Value!, result.Message));
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
