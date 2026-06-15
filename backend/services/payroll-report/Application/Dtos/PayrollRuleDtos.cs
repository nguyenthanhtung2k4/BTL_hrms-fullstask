using System;

namespace Hrms.PayrollReport.Application.Dtos;

public record PayrollRuleDto(
    Guid Id,
    string Code,
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OvertimeRate,
    bool IsActive
);

public record CreatePayrollRuleDto(
    string Code,
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OvertimeRate
);

public record UpdatePayrollRuleDto(
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OvertimeRate,
    bool IsActive
);
