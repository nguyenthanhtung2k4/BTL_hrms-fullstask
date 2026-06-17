using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class WorkSchedule : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public DateOnly WorkDate { get; set; }
    public string Status { get; set; } = "Planned";

    // Navigation properties
    public EmployeeProjection Employee { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
}
