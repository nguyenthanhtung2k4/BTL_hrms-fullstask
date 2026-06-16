using System;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    public string EventName { get; set; } = string.Empty;
    public int EventVersion { get; set; } = 1;
    public string Payload { get; set; } = string.Empty;
    public Guid? CorrelationId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public string Status { get; set; } = "Pending";
}
