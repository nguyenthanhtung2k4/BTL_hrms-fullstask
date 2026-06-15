using System.Collections.Generic;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class Position : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
