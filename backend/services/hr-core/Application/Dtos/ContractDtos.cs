using System;

namespace Hrms.HrCore.Application.Dtos;

public record ContractDto(
    Guid Id,
    string ContractNumber,
    Guid EmployeeId,
    string EmployeeName,
    string ContractType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BaseSalary,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateContractDto(
    string ContractNumber,
    Guid EmployeeId,
    string ContractType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BaseSalary
);

public record UpdateContractDto(
    string ContractType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BaseSalary,
    string Status
);
