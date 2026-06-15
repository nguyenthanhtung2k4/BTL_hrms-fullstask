using Microsoft.Extensions.DependencyInjection;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.PayrollReport.Application.Services;

namespace Hrms.PayrollReport.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPayrollRuleService, PayrollRuleService>();
        services.AddScoped<IPayrollPeriodService, PayrollPeriodService>();
        services.AddScoped<IAllowanceService, AllowanceService>();
        services.AddScoped<IDeductionService, DeductionService>();
        services.AddScoped<IPayslipService, PayslipService>();
        services.AddScoped<IReportService, ReportService>();
        
        return services;
    }
}
