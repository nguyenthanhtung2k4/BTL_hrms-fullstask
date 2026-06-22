using System;
using System.Collections.Generic;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class User : AuditableEntity
{
    public Guid? EmployeeId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }

    // Navigation properties
    public Employee? Employee { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
