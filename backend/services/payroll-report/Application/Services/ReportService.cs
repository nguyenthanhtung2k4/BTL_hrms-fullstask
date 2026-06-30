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

        // Lấy tất cả phiếu lương kèm theo chi tiết Items
        var payslips = await _dbContext.Payslips
            .Include(p => p.Employee)
            .Include(p => p.Items)
            .Where(p => p.PayrollPeriodId == periodId)
            .ToListAsync();

        var departments = await _dbContext.DepartmentProjections.ToListAsync();
        var deptSummaries = new List<DepartmentSummaryDto>();

        foreach (var dept in departments)
        {
            var deptPayslips = payslips
                .Where(p => p.Employee != null && p.Employee.DepartmentId == dept.Id)
                .ToList();

            if (deptPayslips.Count == 0)
            {
                deptSummaries.Add(new DepartmentSummaryDto(
                    dept.Id, dept.Code, dept.Name, 0, 0, 0, 0, 0, 0, 0
                ));
                continue;
            }

            deptSummaries.Add(new DepartmentSummaryDto(
                DepartmentId: dept.Id,
                DepartmentCode: dept.Code,
                DepartmentName: dept.Name,
                EmployeeCount: deptPayslips.Count,
                // SỬA TẠI ĐÂY: Cộng tổng "Lương thực nhận" từ Items thay vì lương hợp đồng
                TotalBaseSalary: deptPayslips.Sum(p => p.Items.Where(i => i.ItemType == "BasicSalary").Sum(i => i.Amount)),
                TotalWorkedDays: deptPayslips.Sum(p => p.WorkedDays),
                TotalAllowance: deptPayslips.Sum(p => p.Items.Where(i => i.ItemType == "Allowance").Sum(i => i.Amount)),
                TotalDeduction: deptPayslips.Sum(p => p.TotalDeduction),
                TotalGrossSalary: deptPayslips.Sum(p => p.GrossSalary),
                TotalNetSalary: deptPayslips.Sum(p => p.NetSalary)
            ));
        }

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
                // SỬA TẠI ĐÂY
                unassignedPayslips.Sum(p => p.Items.Where(i => i.ItemType == "BasicSalary").Sum(i => i.Amount)),
                unassignedPayslips.Sum(p => p.WorkedDays),
                unassignedPayslips.Sum(p => p.Items.Where(i => i.ItemType == "Allowance").Sum(i => i.Amount)),
                unassignedPayslips.Sum(p => p.TotalDeduction),
                unassignedPayslips.Sum(p => p.GrossSalary),
                unassignedPayslips.Sum(p => p.NetSalary)
            ));
        }

        var report = new PayrollSummaryReportDto(
            PayrollPeriodId: periodId,
            PeriodName: period.Name,
            TotalEmployees: payslips.Count,
            // SỬA TẠI ĐÂY (Tổng toàn công ty)
            TotalBaseSalary: payslips.Sum(p => p.Items.Where(i => i.ItemType == "BasicSalary").Sum(i => i.Amount)),
            TotalAllowance: payslips.Sum(p => p.Items.Where(i => i.ItemType == "Allowance").Sum(i => i.Amount)),
            TotalGrossSalary: payslips.Sum(p => p.GrossSalary),
            TotalDeduction: payslips.Sum(p => p.TotalDeduction),
            TotalNetSalary: payslips.Sum(p => p.NetSalary),
            Departments: deptSummaries
        );

        return Result<PayrollSummaryReportDto>.Success(report, "Successfully generated payroll summary report.");
    }
}