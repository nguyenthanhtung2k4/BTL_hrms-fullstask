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
[Route("api/v1/attendance/records")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<ApiResponse<AttendanceRecordDto>>> CheckIn([FromBody] CheckInRequest request)
    {
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
        [FromQuery] DateOnly? toDate)
    {
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
