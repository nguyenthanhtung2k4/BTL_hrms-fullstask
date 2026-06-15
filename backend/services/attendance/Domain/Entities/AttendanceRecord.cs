using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class AttendanceRecord : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? WorkScheduleId { get; set; }
    public Guid ShiftId { get; set; }
    public DateOnly WorkDate { get; set; }
    public DateTime CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public int WorkedMinutes { get; set; } = 0;
    public string Status { get; set; } = "CheckedIn";

    // Navigation properties
    public EmployeeProjection Employee { get; set; } = null!;
    public WorkSchedule? WorkSchedule { get; set; }
    public Shift Shift { get; set; } = null!;
}
