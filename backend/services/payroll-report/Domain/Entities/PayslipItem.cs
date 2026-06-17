using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class PayslipItem : BaseEntity
{
    public Guid PayslipId { get; set; }
    public string ItemType { get; set; } = string.Empty; // BasicSalary, Allowance, Deduction
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? SourceType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Payslip? Payslip { get; set; }
}
