using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class AttendanceProjection : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public int WorkedMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public EmployeeProjection? Employee { get; set; }
}
