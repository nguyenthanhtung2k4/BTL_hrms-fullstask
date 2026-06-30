using System;

namespace Hrms.PayrollReport.Application.Dtos;

public record PayrollPeriodDto(
    Guid Id,
    string Code,
    string Name,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal StandardWorkDays,
    Guid PayrollRuleId,
    string Status,
    DateTime? ClosedAt,
    DateTime CreatedAt
);

public record CreatePayrollPeriodDto(
    string Code,
    string Name,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal StandardWorkDays,
    Guid PayrollRuleId
);

public record UpdatePayrollPeriodDto(
    string Code,
    string Name,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal StandardWorkDays,
    Guid PayrollRuleId
);
