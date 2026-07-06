using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class EmployeeProjection : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public DepartmentProjection? Department { get; set; }
    public PositionProjection? Position { get; set; }
    public EmployeeProjection? Manager { get; set; }
}
