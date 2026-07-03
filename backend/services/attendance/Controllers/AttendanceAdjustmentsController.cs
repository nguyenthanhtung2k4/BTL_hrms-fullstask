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
[Route("api/v1/attendance/adjustments")]
[Authorize]
public class AttendanceAdjustmentsController : ControllerBase
{
    private readonly IAttendanceAdjustmentService _adjustmentService;
    private readonly AttendanceDbContext _dbContext;

    public AttendanceAdjustmentsController(IAttendanceAdjustmentService adjustmentService, AttendanceDbContext dbContext)
    {
        _adjustmentService = adjustmentService;
        _dbContext = dbContext;
    }

    [HttpPost("me")]
    public async Task<ActionResult<ApiResponse<AttendanceAdjustmentDto>>> CreateMyAdjustment([FromBody] CreateAdjustmentRequest request)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<AttendanceAdjustmentDto>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _adjustmentService.CreateAsync(employeeId, request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<AttendanceAdjustmentDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<AttendanceAdjustmentDto>.Ok(result.Value!, result.Message));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceAdjustmentDto>>>> GetMyAdjustments()
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<IEnumerable<AttendanceAdjustmentDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _adjustmentService.GetPersonalRequestsAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<AttendanceAdjustmentDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceAdjustmentDto>>>> GetAdjustments(
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status)
    {
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        var isManager = User.IsInRole("Manager");

        if (!isPrivileged && !isManager)
        {
            return Forbid();
        }

        Guid? departmentId = null;

        if (isManager && !isPrivileged)
        {
            var currentEmpId = GetCurrentEmployeeId();
            if (currentEmpId == Guid.Empty)
            {
                return BadRequest(ApiResponse<IEnumerable<AttendanceAdjustmentDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
            }

            var managerEmp = await _dbContext.EmployeeProjections.FindAsync(currentEmpId);
            departmentId = managerEmp?.DepartmentId;

            if (employeeId.HasValue)
            {
                var targetEmp = await _dbContext.EmployeeProjections.FindAsync(employeeId.Value);
                if (targetEmp == null || (targetEmp.DepartmentId != departmentId && targetEmp.ManagerEmployeeId != currentEmpId))
                {
                    return Forbid();
                }
            }
        }

        var result = await _adjustmentService.GetRequestsAsync(departmentId, employeeId, status);
        return Ok(ApiResponse<IEnumerable<AttendanceAdjustmentDto>>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<ApiResponse<AttendanceAdjustmentDto>>> Approve(Guid id)
    {
        var handledBy = GetCurrentEmployeeId();
        if (handledBy == Guid.Empty)
        {
            return BadRequest(ApiResponse<AttendanceAdjustmentDto>.Fail("InvalidUser", "Approver must be associated with an active Employee account."));
        }

        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        if (!isPrivileged) // Manager check
        {
            var adj = await _dbContext.AttendanceAdjustments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (adj == null)
            {
                return NotFound(ApiResponse<AttendanceAdjustmentDto>.Fail("NotFound", "Adjustment request not found."));
            }

            var managerEmp = await _dbContext.EmployeeProjections.FindAsync(handledBy);
            var managerDeptId = managerEmp?.DepartmentId;

            if (adj.Employee.DepartmentId != managerDeptId && adj.Employee.ManagerEmployeeId != handledBy)
            {
                return Forbid();
            }
        }

        var result = await _adjustmentService.ApproveAsync(id, handledBy);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<AttendanceAdjustmentDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<AttendanceAdjustmentDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<ApiResponse<AttendanceAdjustmentDto>>> Reject(Guid id)
    {
        var handledBy = GetCurrentEmployeeId();
        if (handledBy == Guid.Empty)
        {
            return BadRequest(ApiResponse<AttendanceAdjustmentDto>.Fail("InvalidUser", "Approver must be associated with an active Employee account."));
        }

        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        if (!isPrivileged) // Manager check
        {
            var adj = await _dbContext.AttendanceAdjustments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (adj == null)
            {
                return NotFound(ApiResponse<AttendanceAdjustmentDto>.Fail("NotFound", "Adjustment request not found."));
            }

            var managerEmp = await _dbContext.EmployeeProjections.FindAsync(handledBy);
            var managerDeptId = managerEmp?.DepartmentId;

            if (adj.Employee.DepartmentId != managerDeptId && adj.Employee.ManagerEmployeeId != handledBy)
            {
                return Forbid();
            }
        }

        var result = await _adjustmentService.RejectAsync(id, handledBy);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<AttendanceAdjustmentDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<AttendanceAdjustmentDto>.Ok(result.Value!, result.Message));
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
