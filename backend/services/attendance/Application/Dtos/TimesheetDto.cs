using System;

namespace Hrms.Attendance.Application.Dtos;

public record TimesheetDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    int Year,
    int Month,
    int TotalWorkedMinutes,
    decimal PaidLeaveDays,
    decimal UnpaidLeaveDays,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
