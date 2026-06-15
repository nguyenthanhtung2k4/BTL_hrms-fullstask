using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public Guid? ApprovedByEmployeeId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    public EmployeeProjection Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
    public EmployeeProjection? ApprovedBy { get; set; }
}
