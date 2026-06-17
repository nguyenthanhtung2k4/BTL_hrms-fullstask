using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Application.Interfaces;

public interface IPayrollPeriodService
{
    Task<Result<IEnumerable<PayrollPeriodDto>>> GetAllAsync();
    Task<Result<PayrollPeriodDto>> GetByIdAsync(Guid id);
    Task<Result<PayrollPeriodDto>> CreateAsync(CreatePayrollPeriodDto dto);
    Task<Result<PayrollPeriodDto>> UpdateAsync(Guid id, UpdatePayrollPeriodDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> CalculateAsync(Guid id);
    Task<Result> CloseAsync(Guid id, Guid closedBy);
}
