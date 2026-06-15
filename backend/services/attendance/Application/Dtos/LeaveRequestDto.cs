using System;

namespace Hrms.Attendance.Application.Dtos;

public record LeaveTypeDto(
    Guid Id,
    string Code,
    string Name,
    bool IsPaid,
    bool IsActive
);

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid LeaveTypeId,
    string LeaveTypeName,
    bool IsPaid,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal TotalDays,
    string? Reason,
    string Status,
    Guid? ApprovedByEmployeeId,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateLeaveRequestDto(
    Guid LeaveTypeId,
    DateOnly FromDate,
    DateOnly ToDate,
    string? Reason
);
