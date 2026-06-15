using System;

namespace Hrms.Attendance.Application.Dtos;

public record WorkScheduleDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid ShiftId,
    string ShiftName,
    DateOnly WorkDate,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateWorkScheduleDto(
    Guid EmployeeId,
    Guid ShiftId,
    DateOnly WorkDate
);

public record UpdateWorkScheduleDto(
    Guid ShiftId,
    DateOnly WorkDate,
    string Status
);
