namespace Hrms.Contracts.Events;

public sealed record EmployeeProjectionPayload(
    Guid EmployeeId,
    string EmployeeCode,
    string FullName,
    string? Email,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? PositionId,
    string? PositionName,
    Guid? ManagerEmployeeId,
    string Status);

public sealed record EmployeeStatusChangedPayload(
    Guid EmployeeId,
    string? OldStatus,
    string NewStatus,
    string? Reason);

public sealed record ContractSalaryPayload(
    Guid ContractId,
    Guid EmployeeId,
    decimal BaseSalary,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status);

