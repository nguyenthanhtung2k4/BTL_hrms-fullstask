using System;

namespace Hrms.Contracts.Events;

public sealed record DepartmentPayload(
    Guid DepartmentId,
    string Code,
    string Name,
    bool IsActive
);
