using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Application.Interfaces;

public interface IPayrollRuleService
{
    Task<Result<IEnumerable<PayrollRuleDto>>> GetAllAsync();
    Task<Result<PayrollRuleDto>> GetByIdAsync(Guid id);
    Task<Result<PayrollRuleDto>> CreateAsync(CreatePayrollRuleDto dto);
    Task<Result<PayrollRuleDto>> UpdateAsync(Guid id, UpdatePayrollRuleDto dto);
    Task<Result> DeleteAsync(Guid id);
}
