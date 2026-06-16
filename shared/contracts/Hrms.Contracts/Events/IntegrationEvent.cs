namespace Hrms.Contracts.Events;

public sealed record IntegrationEvent<TPayload>(
    Guid EventId,
    string EventName,
    int Version,
    DateTimeOffset OccurredAt,
    string SourceService,
    Guid? CorrelationId,
    TPayload Payload);

