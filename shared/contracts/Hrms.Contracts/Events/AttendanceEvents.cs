namespace Hrms.Contracts.Events;

public sealed record AttendanceRecordedPayload(
    Guid AttendanceRecordId,
    Guid EmployeeId,
    DateOnly WorkDate,
    Guid ShiftId,
    DateTimeOffset CheckInAt,
    DateTimeOffset? CheckOutAt,
    int WorkedMinutes,
    string Status);

public sealed record LeaveApprovedPayload(
    Guid LeaveRequestId,
    Guid EmployeeId,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal TotalDays,
    string LeaveType,
    bool Paid,
    Guid ApprovedBy);

