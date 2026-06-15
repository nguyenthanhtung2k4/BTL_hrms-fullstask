using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class EmployeeDeduction : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid PayrollPeriodId { get; set; }
    public Guid DeductionTypeId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public EmployeeProjection? Employee { get; set; }
    public PayrollPeriod? PayrollPeriod { get; set; }
    public DeductionType? DeductionType { get; set; }
}
