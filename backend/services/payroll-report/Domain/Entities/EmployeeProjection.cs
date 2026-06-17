using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class EmployeeProjection : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public DepartmentProjection? Department { get; set; }
    public PositionProjection? Position { get; set; }
    public EmployeeProjection? Manager { get; set; }
}
