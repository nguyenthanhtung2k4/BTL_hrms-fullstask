using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.PayrollReport.Domain.Entities;
using Hrms.PayrollReport.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hrms.PayrollReport.Application.Services;

public class PayslipService : IPayslipService
{
    private readonly PayrollReportDbContext _dbContext;

    public PayslipService(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<PayslipDto>>> GetPayslipsAsync(Guid? periodId, Guid? employeeId, Guid? departmentId)
    {
        var query = _dbContext.Payslips
            .Include(p => p.Employee)
            .Include(p => p.PayrollPeriod)
            .Include(p => p.Items)
            .AsQueryable();

        if (periodId.HasValue)
            query = query.Where(p => p.PayrollPeriodId == periodId.Value);

        if (employeeId.HasValue)
            query = query.Where(p => p.EmployeeId == employeeId.Value);

        if (departmentId.HasValue)
            query = query.Where(p => p.Employee != null && p.Employee.DepartmentId == departmentId.Value);

        var list = await query
            .Select(p => new PayslipDto(
                p.Id,
                p.PayrollPeriodId,
                p.EmployeeId,
                p.Employee != null ? p.Employee.EmployeeCode : string.Empty,
                p.Employee != null ? p.Employee.FullName : string.Empty,
                p.BaseSalary,
                p.WorkedDays,
                p.PaidLeaveDays,
                p.GrossSalary,
                p.TotalDeduction,
                p.NetSalary,
                p.Status,
                p.Items.Select(i => new PayslipItemDto(i.Id, i.ItemType, i.Code, i.Name, i.Amount, i.SourceType)).ToList(),
                p.PayrollPeriod != null ? p.PayrollPeriod.Name : string.Empty,
                p.PayrollPeriod != null ? p.PayrollPeriod.Code : string.Empty
            ))
            .ToListAsync();

        return Result<IEnumerable<PayslipDto>>.Success(list, "Successfully retrieved payslips.");
    }

    public async Task<Result<PayslipDto>> GetByIdAsync(Guid id)
    {
        var p = await _dbContext.Payslips
            .Include(x => x.Employee)
            .Include(x => x.PayrollPeriod)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return Result<PayslipDto>.Failure("PayslipNotFound", "Payslip not found.");

        var dto = new PayslipDto(
            p.Id, p.PayrollPeriodId, p.EmployeeId,
            p.Employee?.EmployeeCode ?? string.Empty, p.Employee?.FullName ?? string.Empty,
            p.BaseSalary, p.WorkedDays, p.PaidLeaveDays, p.GrossSalary, p.TotalDeduction, p.NetSalary, p.Status,
            p.Items.Select(i => new PayslipItemDto(i.Id, i.ItemType, i.Code, i.Name, i.Amount, i.SourceType)).ToList(),
            p.PayrollPeriod?.Name ?? string.Empty, p.PayrollPeriod?.Code ?? string.Empty
        );

        return Result<PayslipDto>.Success(dto, "Successfully retrieved payslip.");
    }

    public async Task<Result<PayslipDto>> GetMyPayslipAsync(Guid employeeId, Guid periodId)
    {
        var p = await _dbContext.Payslips
            .Include(x => x.Employee)
            .Include(x => x.PayrollPeriod)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.PayrollPeriodId == periodId);

        if (p == null) return Result<PayslipDto>.Failure("PayslipNotFound", "Payslip not found for the specified period.");

        var dto = new PayslipDto(
            p.Id, p.PayrollPeriodId, p.EmployeeId,
            p.Employee?.EmployeeCode ?? string.Empty, p.Employee?.FullName ?? string.Empty,
            p.BaseSalary, p.WorkedDays, p.PaidLeaveDays, p.GrossSalary, p.TotalDeduction, p.NetSalary, p.Status,
            p.Items.Select(i => new PayslipItemDto(i.Id, i.ItemType, i.Code, i.Name, i.Amount, i.SourceType)).ToList(),
            p.PayrollPeriod?.Name ?? string.Empty, p.PayrollPeriod?.Code ?? string.Empty
        );

        return Result<PayslipDto>.Success(dto, "Successfully retrieved your payslip.");
    }

    public async Task<Result> CalculatePeriodPayslipsAsync(Guid periodId)
    {
        var period = await _dbContext.PayrollPeriods
            .Include(p => p.PayrollRule)
            .FirstOrDefaultAsync(p => p.Id == periodId);

        if (period == null) return Result.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        if (period.Status == "Closed") return Result.Failure("PeriodClosed", "Cannot calculate payroll for a closed payroll period.");

        var rule = period.PayrollRule;
        if (rule == null) return Result.Failure("PayrollRuleNotFound", "Payroll rule not assigned to this period.");

        var employees = await _dbContext.EmployeeProjections.Where(e => e.Status == "Active").ToListAsync();
        var salaryProjections = await _dbContext.EmployeeSalaryProjections.Where(s => s.Status == "Active" && s.EffectiveFrom <= period.ToDate).ToListAsync();
        var attendances = await _dbContext.AttendanceProjections.Where(a => a.Status == "Completed" && a.WorkDate >= period.FromDate && a.WorkDate <= period.ToDate).ToListAsync();
        var leaves = await _dbContext.LeaveProjections.Where(l => l.FromDate <= period.ToDate && l.ToDate >= period.FromDate).ToListAsync();

        var periodAllowances = await _dbContext.EmployeeAllowances.Include(a => a.AllowanceType).Where(a => a.PayrollPeriodId == periodId).ToListAsync();
        var periodDeductions = await _dbContext.EmployeeDeductions.Include(d => d.DeductionType).Where(d => d.PayrollPeriodId == periodId).ToListAsync();

        var existingPayslips = await _dbContext.Payslips.Include(p => p.Items).Where(p => p.PayrollPeriodId == periodId).ToListAsync();

        foreach (var ps in existingPayslips) _dbContext.PayslipItems.RemoveRange(ps.Items);
        _dbContext.Payslips.RemoveRange(existingPayslips);
        await _dbContext.SaveChangesAsync();

        foreach (var employee in employees)
        {
            var salaryProj = salaryProjections.Where(s => s.EmployeeId == employee.Id).OrderByDescending(s => s.EffectiveFrom).FirstOrDefault();
            if (salaryProj == null) continue;

            var baseSalary = salaryProj.BaseSalary;

            // 1. Tính toán ngày công
            var empAttendances = attendances.Where(a => a.EmployeeId == employee.Id).ToList();
            var totalWorkedMinutes = empAttendances.Sum(a => a.WorkedMinutes);
            var workedHours = totalWorkedMinutes / 60m;
            var workedDays = rule.WorkDayHours > 0 ? (workedHours / rule.WorkDayHours) : 0;

            decimal paidLeaveDays = 0;
            var empLeaves = leaves.Where(l => l.EmployeeId == employee.Id).ToList();
            foreach (var leave in empLeaves)
            {
                var overlapStart = leave.FromDate < period.FromDate ? period.FromDate : leave.FromDate;
                var overlapEnd = leave.ToDate > period.ToDate ? period.ToDate : leave.ToDate;
                if (overlapStart <= overlapEnd)
                {
                    int overlapDays = overlapEnd.DayNumber - overlapStart.DayNumber + 1;
                    int totalLeaveDays = leave.ToDate.DayNumber - leave.FromDate.DayNumber + 1;
                    decimal ratioL = totalLeaveDays > 0 ? ((decimal)overlapDays / totalLeaveDays) : 1;
                    if (ratioL > 1) ratioL = 1;
                    if (leave.IsPaid) paidLeaveDays += (leave.TotalDays * ratioL);
                }
            }

            var totalEffectiveWorkedDays = workedDays + (rule.PaidLeaveCountsAsWork ? paidLeaveDays : 0);

            // XÓA LỖI: Không ép cứng totalEffectiveWorkedDays = standardDays nữa
            decimal standardDays = period.StandardWorkDays > 0 ? (decimal)period.StandardWorkDays : (rule.StandardWorkingDays > 0 ? rule.StandardWorkingDays : 22m);

            // 2. Tính lương cơ bản thực nhận
            decimal workRatio = standardDays > 0 ? (totalEffectiveWorkedDays / standardDays) : 0;
            decimal baseSalaryByWork = Math.Round(baseSalary * workRatio, 0);

            // 3. Tính phụ cấp
            var empAllowances = periodAllowances.Where(a => a.EmployeeId == employee.Id).ToList();
            decimal totalAllowance = 0;
            var proratedAllowances = new List<(string Code, string Name, decimal Amount, string Source)>();

            foreach (var al in empAllowances)
            {
                // Các phụ cấp thường gán cứng tiền chứ không nhân tỷ lệ ngày làm (tuỳ nghiệp vụ, nhưng để tránh lỗi 1.4tr, cứ giữ nguyên số tiền gốc)
                decimal proratedAmt = Math.Round(al.Amount, 0);
                totalAllowance += proratedAmt;
                proratedAllowances.Add((al.AllowanceType?.Code ?? "ALLOWANCE", al.AllowanceType?.Name ?? "Phụ cấp", proratedAmt, "Allowance"));
            }

            // Phụ cấp thâm niên
            decimal seniorityAllowanceAmt = 0;
            if (employee.HireDate < period.ToDate.ToDateTime(TimeOnly.MinValue))
            {
                var seniorityYears = (int)Math.Floor((period.ToDate.ToDateTime(TimeOnly.MinValue) - employee.HireDate).TotalDays / 365.25);
                if (seniorityYears >= 1) seniorityAllowanceAmt = seniorityYears * 200000m;
            }
            totalAllowance += seniorityAllowanceAmt;

            // 4. Tính Bảo hiểm xã hội
            decimal bhxhAmount = 0, bhytAmount = 0, bhtnAmount = 0;
            if (totalEffectiveWorkedDays >= 14)
            {
                decimal insuranceSalaryForSocialAndHealth = Math.Min(baseSalary, 46800000m);
                decimal insuranceSalaryForUnemployment = Math.Min(baseSalary, 99200000m);
                bhxhAmount = Math.Round(insuranceSalaryForSocialAndHealth * 0.08m, 0);
                bhytAmount = Math.Round(insuranceSalaryForSocialAndHealth * 0.015m, 0);
                bhtnAmount = Math.Round(insuranceSalaryForUnemployment * 0.01m, 0);
            }
            decimal totalInsuranceDeduction = bhxhAmount + bhytAmount + bhtnAmount;

            // 5. Khấu trừ & Thuế TNCN
            decimal grossSalary = baseSalaryByWork + totalAllowance; // Đã chuẩn: Gross = Lương ngày công + Phụ cấp

            var empDeductions = periodDeductions.Where(d => d.EmployeeId == employee.Id).ToList();
            decimal otherDeductions = empDeductions.Sum(d => d.Amount);

            decimal personalDeduction = 11000000m;
            decimal assessableIncome = grossSalary - totalInsuranceDeduction - personalDeduction - otherDeductions;
            if (assessableIncome < 0) assessableIncome = 0;
            decimal pitAmount = CalculatePIT(assessableIncome);

            decimal totalDeduction = totalInsuranceDeduction + pitAmount + otherDeductions;
            decimal netSalary = grossSalary - totalDeduction;
            if (netSalary < 0) netSalary = 0;

            // 6. Lưu vào DB
            var payslip = new Payslip
            {
                Id = Guid.NewGuid(),
                PayrollPeriodId = periodId,
                EmployeeId = employee.Id,
                BaseSalary = baseSalary,
                WorkedDays = Math.Round(workedDays, 2), // Làm tròn hiển thị 2 số thập phân
                PaidLeaveDays = Math.Round(paidLeaveDays, 2),
                GrossSalary = grossSalary,
                TotalDeduction = totalDeduction,
                NetSalary = netSalary,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "BasicSalary", Code = "BASIC_SALARY", Name = "Lương cơ bản theo ngày công", Amount = baseSalaryByWork, SourceType = null, CreatedAt = DateTime.UtcNow });

            foreach (var allowance in proratedAllowances)
                payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "Allowance", Code = allowance.Code, Name = allowance.Name, Amount = allowance.Amount, SourceType = allowance.Source, CreatedAt = DateTime.UtcNow });

            if (seniorityAllowanceAmt > 0)
                payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "Allowance", Code = "ALLOWANCE_SENIORITY", Name = "Phụ cấp thâm niên", Amount = seniorityAllowanceAmt, SourceType = "Seniority", CreatedAt = DateTime.UtcNow });

            foreach (var deduction in empDeductions)
                payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "Deduction", Code = deduction.DeductionType?.Code ?? "DEDUCTION", Name = deduction.DeductionType?.Name ?? "Khấu trừ", Amount = deduction.Amount, SourceType = "Deduction", CreatedAt = DateTime.UtcNow });

            if (bhxhAmount > 0) payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "Deduction", Code = "DEDUCTION_BHXH", Name = "Bảo hiểm xã hội (8%)", Amount = bhxhAmount, SourceType = "Insurance", CreatedAt = DateTime.UtcNow });
            if (bhytAmount > 0) payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "Deduction", Code = "DEDUCTION_BHYT", Name = "Bảo hiểm y tế (1.5%)", Amount = bhytAmount, SourceType = "Insurance", CreatedAt = DateTime.UtcNow });
            if (bhtnAmount > 0) payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "Deduction", Code = "DEDUCTION_BHTN", Name = "Bảo hiểm thất nghiệp (1%)", Amount = bhtnAmount, SourceType = "Insurance", CreatedAt = DateTime.UtcNow });
            if (pitAmount > 0) payslip.Items.Add(new PayslipItem { Id = Guid.NewGuid(), PayslipId = payslip.Id, ItemType = "Deduction", Code = "DEDUCTION_PIT", Name = "Thuế thu nhập cá nhân (TNCN)", Amount = pitAmount, SourceType = "Tax", CreatedAt = DateTime.UtcNow });

            _dbContext.Payslips.Add(payslip);
        }

        await _dbContext.SaveChangesAsync();
        return Result.Success("Successfully calculated payslips for the period.");
    }

    private decimal CalculatePIT(decimal assessableIncome)
    {
        if (assessableIncome <= 0) return 0;

        decimal tax = 0;
        if (assessableIncome <= 5000000m) tax = assessableIncome * 0.05m;
        else if (assessableIncome <= 10000000m) tax = (5000000m * 0.05m) + ((assessableIncome - 5000000m) * 0.10m);
        else if (assessableIncome <= 18000000m) tax = (5000000m * 0.05m) + (5000000m * 0.10m) + ((assessableIncome - 10000000m) * 0.15m);
        else if (assessableIncome <= 32000000m) tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + ((assessableIncome - 18000000m) * 0.20m);
        else if (assessableIncome <= 52000000m) tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + (14000000m * 0.20m) + ((assessableIncome - 32000000m) * 0.25m);
        else if (assessableIncome <= 80000000m) tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + (14000000m * 0.20m) + (20000000m * 0.25m) + ((assessableIncome - 52000000m) * 0.30m);
        else tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + (14000000m * 0.20m) + (20000000m * 0.25m) + (28000000m * 0.30m) + ((assessableIncome - 80000000m) * 0.35m);

        return Math.Round(tax, 0);
    }
}