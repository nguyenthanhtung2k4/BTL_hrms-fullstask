using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class PositionProjection : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
}
