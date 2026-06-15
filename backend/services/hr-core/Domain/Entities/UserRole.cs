using System;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
