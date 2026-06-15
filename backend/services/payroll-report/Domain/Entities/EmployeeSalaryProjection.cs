using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class EmployeeSalaryProjection : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ContractId { get; set; }
    public decimal BaseSalary { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public EmployeeProjection? Employee { get; set; }
}
