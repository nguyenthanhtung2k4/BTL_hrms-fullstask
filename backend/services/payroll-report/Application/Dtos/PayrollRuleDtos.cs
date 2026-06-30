using System;

namespace Hrms.PayrollReport.Application.Dtos;

public record PayrollRuleDto(
    Guid Id,
    string Code,
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OtMultiplierWeekday,
    decimal OtMultiplierWeekend,
    decimal OtMultiplierHoliday,
    decimal StandardWorkingDays,
    decimal LatePenaltyRule,
    bool IsActive,
    decimal OvertimeRate
);

public record CreatePayrollRuleDto(
    string Code,
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OtMultiplierWeekday,
    decimal OtMultiplierWeekend,
    decimal OtMultiplierHoliday,
    decimal StandardWorkingDays,
    decimal LatePenaltyRule,
    decimal OvertimeRate
);

public record UpdatePayrollRuleDto(
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OtMultiplierWeekday,
    decimal OtMultiplierWeekend,
    decimal OtMultiplierHoliday,
    decimal StandardWorkingDays,
    decimal LatePenaltyRule,
    bool IsActive,
    decimal OvertimeRate
);