using System;
using System.Collections.Generic;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class Payslip : BaseEntity
{
    public Guid PayrollPeriodId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal WorkedDays { get; set; } = 0;
    public decimal PaidLeaveDays { get; set; } = 0;
    public decimal GrossSalary { get; set; } = 0;
    public decimal TotalDeduction { get; set; } = 0;
    public decimal NetSalary { get; set; } = 0;
    public string Status { get; set; } = "Draft"; // Draft, Paid, Closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public PayrollPeriod? PayrollPeriod { get; set; }
    public EmployeeProjection? Employee { get; set; }
    public ICollection<PayslipItem> Items { get; set; } = new List<PayslipItem>();
}
