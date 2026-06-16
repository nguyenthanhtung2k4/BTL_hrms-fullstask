namespace Hrms.Contracts.Events;

public sealed record PayrollClosedPayload(
    Guid PayrollPeriodId,
    string PeriodName,
    DateOnly FromDate,
    DateOnly ToDate,
    DateTimeOffset ClosedAt,
    Guid ClosedBy);

