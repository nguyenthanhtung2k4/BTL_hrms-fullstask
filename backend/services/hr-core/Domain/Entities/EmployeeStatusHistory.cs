using System;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class EmployeeStatusHistory : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
