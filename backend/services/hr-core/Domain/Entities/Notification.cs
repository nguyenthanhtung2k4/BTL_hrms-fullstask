using System;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class Notification : AuditableEntity
{
    public Guid? EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    // Navigation property
    public Employee? Employee { get; set; }
}
