using System;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class Contract : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } = "Active";


    // Navigation properties
    public Employee Employee { get; set; } = null!;
}
