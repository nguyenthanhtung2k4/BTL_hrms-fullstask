using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class AttendanceAdjustment : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime? ProposedCheckIn { get; set; }
    public DateTime? ProposedCheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public Guid? HandledByEmployeeId { get; set; }
    public DateTime? HandledAt { get; set; }

    // Navigation properties
    public EmployeeProjection Employee { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
    public EmployeeProjection? HandledBy { get; set; }
}
