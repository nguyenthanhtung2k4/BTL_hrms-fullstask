using System;
using System.Collections.Generic;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class Employee : AuditableEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;
    public Position Position { get; set; } = null!;
    public Employee? Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public User? User { get; set; }
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<EmployeeStatusHistory> StatusHistories { get; set; } = new List<EmployeeStatusHistory>();
}
