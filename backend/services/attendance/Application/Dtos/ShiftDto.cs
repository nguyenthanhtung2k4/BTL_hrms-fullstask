using System;

namespace Hrms.Attendance.Application.Dtos;

public record ShiftDto(
    Guid Id,
    string Code,
    string Name,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int BreakMinutes,
    bool IsOvernight,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateShiftDto(
    string Code,
    string Name,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int BreakMinutes,
    bool IsOvernight
);

public record UpdateShiftDto(
    string Name,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int BreakMinutes,
    bool IsOvernight,
    bool IsActive
);
