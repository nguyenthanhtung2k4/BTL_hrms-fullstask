using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hrms.Attendance.Application.Dtos;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.Attendance.Controllers;

[ApiController]
[Route("api/v1/attendance/leave-requests")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveService;

    public LeaveRequestsController(ILeaveRequestService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeaveTypeDto>>>> GetLeaveTypes()
    {
        var result = await _leaveService.GetLeaveTypesAsync();
        return Ok(ApiResponse<IEnumerable<LeaveTypeDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequestDto>>>> GetMyRequests()
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<IEnumerable<LeaveRequestDto>>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _leaveService.GetPersonalRequestsAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<LeaveRequestDto>>.Ok(result.Value!, result.Message));
    }

    [HttpPost("me")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> CreateMyRequest([FromBody] CreateLeaveRequestDto dto)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _leaveService.CreateAsync(employeeId, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<LeaveRequestDto>.Ok(result.Value!, result.Message));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequestDto>>>> GetRequests(
        [FromQuery] string? status,
        [FromQuery] Guid? departmentId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        var result = await _leaveService.GetRequestsAsync(status, departmentId, fromDate, toDate);
        return Ok(ApiResponse<IEnumerable<LeaveRequestDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> GetById(Guid id)
    {
        var result = await _leaveService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<LeaveRequestDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<LeaveRequestDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Approve(Guid id)
    {
        var approvedBy = GetCurrentEmployeeId();
        if (approvedBy == Guid.Empty)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail("InvalidUser", "Approver must be associated with an active Employee account."));
        }

        var result = await _leaveService.ApproveAsync(id, approvedBy);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<LeaveRequestDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Reject(Guid id)
    {
        var approvedBy = GetCurrentEmployeeId();
        if (approvedBy == Guid.Empty)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail("InvalidUser", "Approver must be associated with an active Employee account."));
        }

        var result = await _leaveService.RejectAsync(id, approvedBy);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<LeaveRequestDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Cancel(Guid id)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _leaveService.CancelAsync(id, employeeId);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<LeaveRequestDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<LeaveRequestDto>.Ok(result.Value!, result.Message));
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
