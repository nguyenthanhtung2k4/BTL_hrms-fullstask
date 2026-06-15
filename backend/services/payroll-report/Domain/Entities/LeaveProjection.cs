using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class LeaveProjection : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal TotalDays { get; set; }
    public bool IsPaid { get; set; }
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public EmployeeProjection? Employee { get; set; }
}
