using System;

namespace Hrms.Attendance.Application.Dtos;

public record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid? WorkScheduleId,
    Guid ShiftId,
    string ShiftName,
    DateOnly WorkDate,
    DateTime CheckInAt,
    DateTime? CheckOutAt,
    int WorkedMinutes,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CheckInRequest(string ShiftCode);
