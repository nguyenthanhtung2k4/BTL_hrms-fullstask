using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class PayrollRule : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal WorkDayHours { get; set; } = 8;
    public bool PaidLeaveCountsAsWork { get; set; } = true;
    public decimal OvertimeRate { get; set; } = 1.5m;
    public bool IsActive { get; set; } = true;
}
