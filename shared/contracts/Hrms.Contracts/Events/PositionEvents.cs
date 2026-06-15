using System;

namespace Hrms.Contracts.Events;

public sealed record PositionPayload(
    Guid PositionId,
    string Code,
    string Name,
    bool IsActive
);
