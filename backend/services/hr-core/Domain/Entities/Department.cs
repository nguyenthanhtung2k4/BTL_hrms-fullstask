using System;
using System.Collections.Generic;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class Department : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
    public Employee? ManagerEmployee { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
