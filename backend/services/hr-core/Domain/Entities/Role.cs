using System;
using System.Collections.Generic;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
