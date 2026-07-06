using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Application.Interfaces;

public interface IAllowanceService
{
    Task<Result<IEnumerable<EmployeeAllowanceDto>>> GetAllowancesAsync(Guid? employeeId, Guid? periodId);
    Task<Result<EmployeeAllowanceDto>> GetByIdAsync(Guid id);
    Task<Result<EmployeeAllowanceDto>> CreateAsync(CreateEmployeeAllowanceDto dto);
    Task<Result<EmployeeAllowanceDto>> UpdateAsync(Guid id, UpdateEmployeeAllowanceDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<IEnumerable<AllowanceTypeDto>>> GetAllowanceTypesAsync();
    Task<Result<AllowanceTypeDto>> CreateAllowanceTypeAsync(string name);
}
