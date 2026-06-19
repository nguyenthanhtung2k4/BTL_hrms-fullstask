using System;

namespace Hrms.PayrollReport.Application.Dtos;

public record EmployeeAllowanceDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    Guid PayrollPeriodId,
    string PeriodName,
    Guid AllowanceTypeId,
    string AllowanceTypeCode,
    string AllowanceTypeName,
    decimal Amount,
    string? Note,
    DateTime CreatedAt
);

public record CreateEmployeeAllowanceDto(
    Guid EmployeeId,
    Guid PayrollPeriodId,
    Guid AllowanceTypeId,
    decimal Amount,
    string? Note
);

public record UpdateEmployeeAllowanceDto(
    decimal Amount,
    string? Note
);

public record AllowanceTypeDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive
);

// Deductions
public record EmployeeDeductionDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    Guid PayrollPeriodId,
    string PeriodName,
    Guid DeductionTypeId,
    string DeductionTypeCode,
    string DeductionTypeName,
    decimal Amount,
    string? Note,
    DateTime CreatedAt
);

public record CreateEmployeeDeductionDto(
    Guid EmployeeId,
    Guid PayrollPeriodId,
    Guid DeductionTypeId,
    decimal Amount,
    string? Note
);

public record UpdateEmployeeDeductionDto(
    decimal Amount,
    string? Note
);

public record DeductionTypeDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive
);
