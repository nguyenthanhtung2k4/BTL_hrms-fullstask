using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class Timesheet : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalWorkedMinutes { get; set; } = 0;
    public decimal PaidLeaveDays { get; set; } = 0;
    public decimal UnpaidLeaveDays { get; set; } = 0;
    public string Status { get; set; } = "Draft";

    // Navigation properties
    public EmployeeProjection Employee { get; set; } = null!;
}
