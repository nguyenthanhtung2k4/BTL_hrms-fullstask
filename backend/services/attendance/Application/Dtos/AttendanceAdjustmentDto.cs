using System;

namespace Hrms.Attendance.Application.Dtos;

public record AttendanceAdjustmentDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly WorkDate,
    Guid ShiftId,
    string ShiftName,
    DateTime? ProposedCheckIn,
    DateTime? ProposedCheckOut,
    string Reason,
    string Status,
    Guid? HandledByEmployeeId,
    string? HandledByName,
    DateTime? HandledAt,
    DateTime CreatedAt
);

public record CreateAdjustmentRequest(
    DateOnly WorkDate,
    Guid ShiftId,
    DateTime? ProposedCheckIn,
    DateTime? ProposedCheckOut,
    string Reason
);
