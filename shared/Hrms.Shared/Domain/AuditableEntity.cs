using System;

namespace Hrms.Shared.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

public abstract class AuditableEntity<TKey> : BaseEntity<TKey>, IAuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public abstract class AuditableEntity : AuditableEntity<Guid>
{
    protected AuditableEntity()
    {
        Id = Guid.NewGuid();
    }
}

