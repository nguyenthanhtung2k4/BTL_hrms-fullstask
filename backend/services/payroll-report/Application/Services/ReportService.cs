using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.PayrollReport.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hrms.PayrollReport.Application.Services;

public class ReportService : IReportService
{
    private readonly PayrollReportDbContext _dbContext;

    public ReportService(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PayrollSummaryReportDto>> GetSummaryReportAsync(Guid periodId)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(periodId);
        if (period == null)
        {
            return Result<PayrollSummaryReportDto>.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        // Get all payslips for this period
        var payslips = await _dbContext.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollPeriodId == periodId)
            .ToListAsync();

        // Get all departments to make sure we represent them all
        var departments = await _dbContext.DepartmentProjections.ToListAsync();

        var deptSummaries = new List<DepartmentSummaryDto>();

        foreach (var dept in departments)
        {
            var deptPayslips = payslips
                .Where(p => p.Employee != null && p.Employee.DepartmentId == dept.Id)
                .ToList();

            if (deptPayslips.Count == 0)
            {
                // Add empty department info
                deptSummaries.Add(new DepartmentSummaryDto(
                    dept.Id,
                    dept.Code,
                    dept.Name,
                    0, 0, 0, 0, 0, 0
                ));
                continue;
            }

            deptSummaries.Add(new DepartmentSummaryDto(
                DepartmentId: dept.Id,
                DepartmentCode: dept.Code,
                DepartmentName: dept.Name,
                EmployeeCount: deptPayslips.Count,
                TotalBaseSalary: deptPayslips.Sum(p => p.BaseSalary),
                TotalWorkedDays: deptPayslips.Sum(p => p.WorkedDays),
                TotalGrossSalary: deptPayslips.Sum(p => p.GrossSalary),
                TotalDeduction: deptPayslips.Sum(p => p.TotalDeduction),
                TotalNetSalary: deptPayslips.Sum(p => p.NetSalary)
            ));
        }

        // Include any payslips that don't have a department projection mapping just in case
        var unassignedPayslips = payslips
            .Where(p => p.Employee == null || !p.Employee.DepartmentId.HasValue || !departments.Any(d => d.Id == p.Employee.DepartmentId.Value))
            .ToList();

        if (unassignedPayslips.Count > 0)
        {
            deptSummaries.Add(new DepartmentSummaryDto(
                Guid.Empty,
                "UNASSIGNED",
                "Unassigned Department",
                unassignedPayslips.Count,
                unassignedPayslips.Sum(p => p.BaseSalary),
                unassignedPayslips.Sum(p => p.WorkedDays),
                unassignedPayslips.Sum(p => p.GrossSalary),
                unassignedPayslips.Sum(p => p.TotalDeduction),
                unassignedPayslips.Sum(p => p.NetSalary)
            ));
        }

        var report = new PayrollSummaryReportDto(
            PayrollPeriodId: periodId,
            PeriodName: period.Name,
            TotalEmployees: payslips.Count,
            TotalBaseSalary: payslips.Sum(p => p.BaseSalary),
            TotalGrossSalary: payslips.Sum(p => p.GrossSalary),
            TotalDeduction: payslips.Sum(p => p.TotalDeduction),
            TotalNetSalary: payslips.Sum(p => p.NetSalary),
            Departments: deptSummaries
        );

        return Result<PayrollSummaryReportDto>.Success(report, "Successfully generated payroll summary report.");
    }
}
