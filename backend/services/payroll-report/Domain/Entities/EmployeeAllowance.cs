using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class EmployeeAllowance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid PayrollPeriodId { get; set; }
    public Guid AllowanceTypeId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public EmployeeProjection? Employee { get; set; }
    public PayrollPeriod? PayrollPeriod { get; set; }
    public AllowanceType? AllowanceType { get; set; }
}
