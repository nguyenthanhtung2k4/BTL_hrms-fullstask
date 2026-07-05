using System;

namespace Hrms.PayrollReport.Application.Dtos;

public record PayrollRuleDto(
    Guid Id,
    string Code,
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OvertimeRate,
    bool IsActive,
    int GracePeriodMinutes,
    decimal LateDeductionRate,
    decimal WeekendOvertimeRate,
    decimal HolidayOvertimeRate,
    int RoundingMinutes
);

public record CreatePayrollRuleDto(
    string Code,
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OvertimeRate,
    int GracePeriodMinutes,
    decimal LateDeductionRate,
    decimal WeekendOvertimeRate,
    decimal HolidayOvertimeRate,
    int RoundingMinutes
);

public record UpdatePayrollRuleDto(
    string Name,
    decimal WorkDayHours,
    bool PaidLeaveCountsAsWork,
    decimal OvertimeRate,
    bool IsActive,
    int GracePeriodMinutes,
    decimal LateDeductionRate,
    decimal WeekendOvertimeRate,
    decimal HolidayOvertimeRate,
    int RoundingMinutes
);
