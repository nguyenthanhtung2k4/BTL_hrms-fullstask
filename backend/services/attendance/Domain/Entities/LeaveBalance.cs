using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class LeaveBalance : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int Year { get; set; }
    public decimal EntitledDays { get; set; }
    public decimal UsedDays { get; set; }

    public decimal RemainingDays => EntitledDays - UsedDays;

    // Navigation properties
    public EmployeeProjection Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
