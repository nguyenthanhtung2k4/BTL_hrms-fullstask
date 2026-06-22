using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Application.Interfaces;

public interface IDeductionService
{
    Task<Result<IEnumerable<EmployeeDeductionDto>>> GetDeductionsAsync(Guid? employeeId, Guid? periodId);
    Task<Result<EmployeeDeductionDto>> GetByIdAsync(Guid id);
    Task<Result<EmployeeDeductionDto>> CreateAsync(CreateEmployeeDeductionDto dto);
    Task<Result<EmployeeDeductionDto>> UpdateAsync(Guid id, UpdateEmployeeDeductionDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<IEnumerable<DeductionTypeDto>>> GetDeductionTypesAsync();
    Task<Result<DeductionTypeDto>> CreateDeductionTypeAsync(string name);
}
