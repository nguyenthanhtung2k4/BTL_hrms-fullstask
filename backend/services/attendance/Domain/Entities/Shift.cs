using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class Shift : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int BreakMinutes { get; set; }
    public bool IsOvernight { get; set; }
    public bool IsActive { get; set; } = true;
}
