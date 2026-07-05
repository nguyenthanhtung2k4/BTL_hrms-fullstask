using System;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Application.Interfaces;

public interface IReportService
{
    Task<Result<PayrollSummaryReportDto>> GetSummaryReportAsync(Guid periodId, Guid? departmentId = null);
}
