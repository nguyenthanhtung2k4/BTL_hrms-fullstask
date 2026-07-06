using System;
using System.Collections.Generic;

namespace Hrms.PayrollReport.Application.Dtos;

public record PayslipDto(
    Guid Id,
    Guid PayrollPeriodId,
    Guid EmployeeId,
    string EmployeeCode,
    string FullName,
    decimal BaseSalary,
    decimal WorkedDays,
    decimal PaidLeaveDays,
    decimal GrossSalary,
    decimal TotalDeduction,
    decimal NetSalary,
    string Status,
    List<PayslipItemDto> Items,
    string? PeriodName = null,
    string? PeriodCode = null
);

public record PayslipItemDto(
    Guid Id,
    string ItemType,
    string Code,
    string Name,
    decimal Amount,
    string? SourceType
);

public record UpdatePayslipDto(
    decimal WorkedDays,
    decimal PaidLeaveDays
);
