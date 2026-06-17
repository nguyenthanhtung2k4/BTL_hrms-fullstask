using System;
using Hrms.Shared.Domain;

namespace Hrms.Attendance.Domain.Entities;

public class LeaveType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPaid { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
