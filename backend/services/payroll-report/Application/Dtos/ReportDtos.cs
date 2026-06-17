using System;
using System.Collections.Generic;

namespace Hrms.PayrollReport.Application.Dtos;

public record DepartmentSummaryDto(
    Guid DepartmentId,
    string DepartmentCode,
    string DepartmentName,
    int EmployeeCount,
    decimal TotalBaseSalary,
    decimal TotalWorkedDays,
    decimal TotalGrossSalary,
    decimal TotalDeduction,
    decimal TotalNetSalary
);

public record PayrollSummaryReportDto(
    Guid PayrollPeriodId,
    string PeriodName,
    int TotalEmployees,
    decimal TotalBaseSalary,
    decimal TotalGrossSalary,
    decimal TotalDeduction,
    decimal TotalNetSalary,
    List<DepartmentSummaryDto> Departments
);
