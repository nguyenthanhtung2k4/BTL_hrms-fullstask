using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Application.Interfaces;

public interface IPayslipService
{
    Task<Result<IEnumerable<PayslipDto>>> GetPayslipsAsync(Guid? periodId, Guid? employeeId, Guid? departmentId, bool onlyCalculatedOrClosed = false);
    Task<Result<PayslipDto>> GetByIdAsync(Guid id);
    Task<Result<PayslipDto>> GetMyPayslipAsync(Guid employeeId, Guid periodId);
    Task<Result> CalculatePeriodPayslipsAsync(Guid periodId);
    Task<Result<PayslipDto>> UpdatePayslipAsync(Guid id, UpdatePayslipDto dto);
}
