using System;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? ActorUser { get; set; }
}
