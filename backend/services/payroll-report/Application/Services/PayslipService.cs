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
        {
            query = query.Where(p => p.PayrollPeriodId == periodId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(p => p.Employee != null && p.Employee.DepartmentId == departmentId.Value);
        }

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

        if (p == null)
        {
            return Result<PayslipDto>.Failure("PayslipNotFound", "Payslip not found.");
        }

        var dto = new PayslipDto(
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

        if (p == null)
        {
            return Result<PayslipDto>.Failure("PayslipNotFound", "Payslip not found for the specified period.");
        }

        var dto = new PayslipDto(
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
        );

        return Result<PayslipDto>.Success(dto, "Successfully retrieved your payslip.");
    }

    public async Task<Result> CalculatePeriodPayslipsAsync(Guid periodId)
    {
        var period = await _dbContext.PayrollPeriods
            .Include(p => p.PayrollRule)
            .FirstOrDefaultAsync(p => p.Id == periodId);

        if (period == null)
        {
            return Result.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (period.Status == "Closed")
        {
            return Result.Failure("PeriodClosed", "Cannot calculate payroll for a closed payroll period.");
        }

        var rule = period.PayrollRule;
        if (rule == null)
        {
            return Result.Failure("PayrollRuleNotFound", "Payroll rule not assigned to this period.");
        }

        // Get all active employees
        var employees = await _dbContext.EmployeeProjections
            .Where(e => e.Status == "Active")
            .ToListAsync();

        // Get all base salary projections for this period window
        var salaryProjections = await _dbContext.EmployeeSalaryProjections
            .Where(s => s.Status == "Active" && s.EffectiveFrom <= period.ToDate)
            .ToListAsync();

        // Get all completed attendance records for this period window
        var attendances = await _dbContext.AttendanceProjections
            .Where(a => (a.Status == "Completed" || a.Status == "CheckedOut" || a.Status == "Late") && a.WorkDate >= period.FromDate && a.WorkDate <= period.ToDate)
            .ToListAsync();

        // Get all leave projections for this period window
        var leaves = await _dbContext.LeaveProjections
            .Where(l => l.FromDate <= period.ToDate && l.ToDate >= period.FromDate)
            .ToListAsync();

        // Get allowances and deductions
        var periodAllowances = await _dbContext.EmployeeAllowances
            .Include(a => a.AllowanceType)
            .Where(a => a.PayrollPeriodId == periodId)
            .ToListAsync();

        var periodDeductions = await _dbContext.EmployeeDeductions
            .Include(d => d.DeductionType)
            .Where(d => d.PayrollPeriodId == periodId)
            .ToListAsync();

        // Delete existing payslips for this period (items first due to FK constraint)
        var existingPayslips = await _dbContext.Payslips
            .Include(p => p.Items)
            .Where(p => p.PayrollPeriodId == periodId)
            .ToListAsync();
        foreach (var ps in existingPayslips)
        {
            _dbContext.PayslipItems.RemoveRange(ps.Items);
        }
        _dbContext.Payslips.RemoveRange(existingPayslips);
        await _dbContext.SaveChangesAsync();

        foreach (var employee in employees)
        {
            // 1. Get base salary projection
            var salaryProj = salaryProjections
                .Where(s => s.EmployeeId == employee.Id)
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefault();

            if (salaryProj == null)
            {
                // No salary contract configured for this employee, skip or assume 0
                continue;
            }

            var baseSalary = salaryProj.BaseSalary;

            // 2. Calculate worked days
            var empAttendances = attendances.Where(a => a.EmployeeId == employee.Id).ToList();
            var totalWorkedMinutes = empAttendances.Sum(a => a.WorkedMinutes);
            var workedHours = totalWorkedMinutes / 60m;
            var workedDays = rule.WorkDayHours > 0 ? (workedHours / rule.WorkDayHours) : 0;

            // 3. Calculate paid leave days
            var empLeaves = leaves.Where(l => l.EmployeeId == employee.Id).ToList();
            decimal paidLeaveDays = 0;

            foreach (var leave in empLeaves)
            {
                var overlapStart = leave.FromDate < period.FromDate ? period.FromDate : leave.FromDate;
                var overlapEnd = leave.ToDate > period.ToDate ? period.ToDate : leave.ToDate;

                if (overlapStart <= overlapEnd)
                {
                    int overlapDays = overlapEnd.DayNumber - overlapStart.DayNumber + 1;
                    int totalLeaveDays = leave.ToDate.DayNumber - leave.FromDate.DayNumber + 1;
                    
                    decimal ratio = totalLeaveDays > 0 ? ((decimal)overlapDays / totalLeaveDays) : 1;
                    if (ratio > 1) ratio = 1;

                    var overlapActualDays = leave.TotalDays * ratio;
                    if (leave.IsPaid)
                    {
                        paidLeaveDays += overlapActualDays;
                    }
                }
            }

            // Apply rule logic: paid leave counts as work
            var totalEffectiveWorkedDays = workedDays;
            if (rule.PaidLeaveCountsAsWork)
            {
                totalEffectiveWorkedDays += paidLeaveDays;
            }

            // Cap effective worked days at standard work days
            if (totalEffectiveWorkedDays > period.StandardWorkDays)
            {
                totalEffectiveWorkedDays = period.StandardWorkDays;
            }

            // 4. Calculate base salary by work
            decimal baseSalaryByWork = 0;
            if (period.StandardWorkDays > 0)
            {
                baseSalaryByWork = baseSalary * (totalEffectiveWorkedDays / period.StandardWorkDays);
            }

            // 5. Gather allowances
            var empAllowances = periodAllowances.Where(a => a.EmployeeId == employee.Id).ToList();
            decimal totalAllowance = empAllowances.Sum(a => a.Amount);

            // Automatically calculate Seniority Allowance (200,000 VND / year of service)
            decimal seniorityAllowanceAmt = 0;
            int seniorityYears = 0;
            if (employee.HireDate < period.ToDate.ToDateTime(TimeOnly.MinValue))
            {
                var seniorityDays = (period.ToDate.ToDateTime(TimeOnly.MinValue) - employee.HireDate).TotalDays;
                seniorityYears = (int)Math.Floor(seniorityDays / 365.25);
                if (seniorityYears >= 1)
                {
                    seniorityAllowanceAmt = seniorityYears * 200000m;
                }
            }
            totalAllowance += seniorityAllowanceAmt;

            // 6. Calculate Social Insurances (BHXH: 8%, BHYT: 1.5%, BHTN: 1% trích lương nhân viên)
            decimal insuranceSalaryForSocialAndHealth = Math.Min(baseSalary, 46800000m); // Trần 20 lần lương cơ sở (20 * 2,340,000)
            decimal insuranceSalaryForUnemployment = Math.Min(baseSalary, 99200000m);    // Trần 20 lần lương tối thiểu vùng I (20 * 4,960,000)

            decimal bhxhAmount = Math.Round(insuranceSalaryForSocialAndHealth * 0.08m, 0);
            decimal bhytAmount = Math.Round(insuranceSalaryForSocialAndHealth * 0.015m, 0);
            decimal bhtnAmount = Math.Round(insuranceSalaryForUnemployment * 0.01m, 0);
            decimal totalInsuranceDeduction = bhxhAmount + bhytAmount + bhtnAmount;

            // 7. Calculate Personal Income Tax (PIT - Thuế TNCN)
            decimal grossSalary = baseSalaryByWork + totalAllowance;
            decimal personalDeduction = 11000000m; // Giảm trừ bản thân: 11,000,000 VND
            decimal dependentDeduction = 0m;      // Mặc định 0 người phụ thuộc (có thể nâng cấp thêm cột sau)
            
            decimal assessableIncome = grossSalary - totalInsuranceDeduction - personalDeduction - dependentDeduction;
            if (assessableIncome < 0) assessableIncome = 0;
            decimal pitAmount = CalculatePIT(assessableIncome);

            // 8. Gather other custom deductions configured for this period
            var empDeductions = periodDeductions.Where(d => d.EmployeeId == employee.Id).ToList();
            decimal otherDeductions = empDeductions.Sum(d => d.Amount);

            decimal totalDeduction = totalInsuranceDeduction + pitAmount + otherDeductions;
            decimal netSalary = grossSalary - totalDeduction;
            if (netSalary < 0) netSalary = 0;

            // 9. Create Payslip
            var payslip = new Payslip
            {
                Id = Guid.NewGuid(),
                PayrollPeriodId = periodId,
                EmployeeId = employee.Id,
                BaseSalary = baseSalary,
                WorkedDays = workedDays,
                PaidLeaveDays = paidLeaveDays,
                GrossSalary = grossSalary,
                TotalDeduction = totalDeduction,
                NetSalary = netSalary,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            // Basic salary item
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "BasicSalary",
                Code = "BASIC_SALARY",
                Name = "Lương cơ bản theo ngày công",
                Amount = baseSalaryByWork,
                SourceType = null,
                CreatedAt = DateTime.UtcNow
            });

            // Allowance items
            foreach (var allowance in empAllowances)
            {
                payslip.Items.Add(new PayslipItem
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    ItemType = "Allowance",
                    Code = allowance.AllowanceType?.Code ?? "ALLOWANCE",
                    Name = allowance.AllowanceType?.Name ?? "Phụ cấp",
                    Amount = allowance.Amount,
                    SourceType = "Allowance",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Seniority allowance item
            if (seniorityAllowanceAmt > 0)
            {
                payslip.Items.Add(new PayslipItem
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    ItemType = "Allowance",
                    Code = "ALLOWANCE_SENIORITY",
                    Name = $"Phụ cấp thâm niên ({seniorityYears} năm)",
                    Amount = seniorityAllowanceAmt,
                    SourceType = "Seniority",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Custom Deduction items
            foreach (var deduction in empDeductions)
            {
                payslip.Items.Add(new PayslipItem
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    ItemType = "Deduction",
                    Code = deduction.DeductionType?.Code ?? "DEDUCTION",
                    Name = deduction.DeductionType?.Name ?? "Khấu trừ",
                    Amount = deduction.Amount,
                    SourceType = "Deduction",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Auto-generated Insurance Deductions
            if (bhxhAmount > 0)
            {
                payslip.Items.Add(new PayslipItem
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    ItemType = "Deduction",
                    Code = "DEDUCTION_BHXH",
                    Name = "Bảo hiểm xã hội (8%)",
                    Amount = bhxhAmount,
                    SourceType = "Insurance",
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (bhytAmount > 0)
            {
                payslip.Items.Add(new PayslipItem
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    ItemType = "Deduction",
                    Code = "DEDUCTION_BHYT",
                    Name = "Bảo hiểm y tế (1.5%)",
                    Amount = bhytAmount,
                    SourceType = "Insurance",
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (bhtnAmount > 0)
            {
                payslip.Items.Add(new PayslipItem
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    ItemType = "Deduction",
                    Code = "DEDUCTION_BHTN",
                    Name = "Bảo hiểm thất nghiệp (1%)",
                    Amount = bhtnAmount,
                    SourceType = "Insurance",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Auto-generated PIT Deduction
            if (pitAmount > 0)
            {
                payslip.Items.Add(new PayslipItem
                {
                    Id = Guid.NewGuid(),
                    PayslipId = payslip.Id,
                    ItemType = "Deduction",
                    Code = "DEDUCTION_PIT",
                    Name = "Thuế thu nhập cá nhân (TNCN)",
                    Amount = pitAmount,
                    SourceType = "Tax",
                    CreatedAt = DateTime.UtcNow
                });
            }

            _dbContext.Payslips.Add(payslip);
        }

        await _dbContext.SaveChangesAsync();
        return Result.Success("Successfully calculated payslips for the period.");
    }

    public async Task<Result<PayslipDto>> UpdatePayslipAsync(Guid id, UpdatePayslipDto dto)
    {
        var payslip = await _dbContext.Payslips
            .Include(p => p.Items)
            .Include(p => p.Employee)
            .Include(p => p.PayrollPeriod)
            .ThenInclude(pp => pp!.PayrollRule)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payslip == null)
        {
            return Result<PayslipDto>.Failure("PayslipNotFound", "Payslip not found.");
        }

        if (payslip.PayrollPeriod == null)
        {
            return Result<PayslipDto>.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (payslip.PayrollPeriod.Status == "Closed")
        {
            return Result<PayslipDto>.Failure("PeriodClosed", "Cannot edit a payslip in a closed payroll period.");
        }

        var rule = payslip.PayrollPeriod.PayrollRule;
        if (rule == null)
        {
            return Result<PayslipDto>.Failure("PayrollRuleNotFound", "Payroll rule not assigned to this period.");
        }

        payslip.WorkedDays = dto.WorkedDays;
        payslip.PaidLeaveDays = dto.PaidLeaveDays;

        var baseSalary = payslip.BaseSalary;

        // Apply rule logic: paid leave counts as work
        var totalEffectiveWorkedDays = dto.WorkedDays;
        if (rule.PaidLeaveCountsAsWork)
        {
            totalEffectiveWorkedDays += dto.PaidLeaveDays;
        }

        // Cap effective worked days at standard work days
        if (totalEffectiveWorkedDays > payslip.PayrollPeriod.StandardWorkDays)
        {
            totalEffectiveWorkedDays = payslip.PayrollPeriod.StandardWorkDays;
        }

        // Calculate base salary by work
        decimal baseSalaryByWork = 0;
        if (payslip.PayrollPeriod.StandardWorkDays > 0)
        {
            baseSalaryByWork = baseSalary * (totalEffectiveWorkedDays / payslip.PayrollPeriod.StandardWorkDays);
        }

        // Gather allowances and deductions for this employee & period
        var periodAllowances = await _dbContext.EmployeeAllowances
            .Include(a => a.AllowanceType)
            .Where(a => a.PayrollPeriodId == payslip.PayrollPeriodId && a.EmployeeId == payslip.EmployeeId)
            .ToListAsync();
        decimal totalAllowance = periodAllowances.Sum(a => a.Amount);

        // Seniority allowance
        decimal seniorityAllowanceAmt = 0;
        int seniorityYears = 0;
        if (payslip.Employee != null && payslip.Employee.HireDate < payslip.PayrollPeriod.ToDate.ToDateTime(TimeOnly.MinValue))
        {
            var seniorityDays = (payslip.PayrollPeriod.ToDate.ToDateTime(TimeOnly.MinValue) - payslip.Employee.HireDate).TotalDays;
            seniorityYears = (int)Math.Floor(seniorityDays / 365.25);
            if (seniorityYears >= 1)
            {
                seniorityAllowanceAmt = seniorityYears * 200000m;
            }
        }
        totalAllowance += seniorityAllowanceAmt;

        // Calculate Social Insurances (BHXH: 8%, BHYT: 1.5%, BHTN: 1%)
        decimal insuranceSalaryForSocialAndHealth = Math.Min(baseSalary, 46800000m);
        decimal insuranceSalaryForUnemployment = Math.Min(baseSalary, 99200000m);

        decimal bhxhAmount = Math.Round(insuranceSalaryForSocialAndHealth * 0.08m, 0);
        decimal bhytAmount = Math.Round(insuranceSalaryForSocialAndHealth * 0.015m, 0);
        decimal bhtnAmount = Math.Round(insuranceSalaryForUnemployment * 0.01m, 0);
        decimal totalInsuranceDeduction = bhxhAmount + bhytAmount + bhtnAmount;

        // Calculate Personal Income Tax (PIT)
        decimal grossSalary = baseSalaryByWork + totalAllowance;
        decimal personalDeduction = 11000000m;
        
        decimal assessableIncome = grossSalary - totalInsuranceDeduction - personalDeduction;
        if (assessableIncome < 0) assessableIncome = 0;
        decimal pitAmount = CalculatePIT(assessableIncome);

        // Gather other custom deductions
        var periodDeductions = await _dbContext.EmployeeDeductions
            .Include(d => d.DeductionType)
            .Where(d => d.PayrollPeriodId == payslip.PayrollPeriodId && d.EmployeeId == payslip.EmployeeId)
            .ToListAsync();
        decimal otherDeductions = periodDeductions.Sum(d => d.Amount);

        decimal totalDeduction = totalInsuranceDeduction + pitAmount + otherDeductions;
        decimal netSalary = grossSalary - totalDeduction;
        if (netSalary < 0) netSalary = 0;

        payslip.GrossSalary = grossSalary;
        payslip.TotalDeduction = totalDeduction;
        payslip.NetSalary = netSalary;
        payslip.UpdatedAt = DateTime.UtcNow;

        // Clear existing payslip items and save first to avoid concurrency/tracking conflicts
        var existingItems = await _dbContext.PayslipItems.Where(pi => pi.PayslipId == payslip.Id).ToListAsync();
        _dbContext.PayslipItems.RemoveRange(existingItems);
        payslip.Items.Clear();
        await _dbContext.SaveChangesAsync();

        // Re-generate basic salary item
        payslip.Items.Add(new PayslipItem
        {
            Id = Guid.NewGuid(),
            PayslipId = payslip.Id,
            ItemType = "BasicSalary",
            Code = "BASIC_SALARY",
            Name = "Lương cơ bản theo ngày công",
            Amount = baseSalaryByWork,
            SourceType = null,
            CreatedAt = DateTime.UtcNow
        });

        // Allowance items
        foreach (var allowance in periodAllowances)
        {
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "Allowance",
                Code = allowance.AllowanceType?.Code ?? "ALLOWANCE",
                Name = allowance.AllowanceType?.Name ?? "Phụ cấp",
                Amount = allowance.Amount,
                SourceType = "Allowance",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Seniority allowance item
        if (seniorityAllowanceAmt > 0)
        {
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "Allowance",
                Code = "ALLOWANCE_SENIORITY",
                Name = $"Phụ cấp thâm niên ({seniorityYears} năm)",
                Amount = seniorityAllowanceAmt,
                SourceType = "Seniority",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Custom Deduction items
        foreach (var deduction in periodDeductions)
        {
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "Deduction",
                Code = deduction.DeductionType?.Code ?? "DEDUCTION",
                Name = deduction.DeductionType?.Name ?? "Khấu trừ",
                Amount = deduction.Amount,
                SourceType = "Deduction",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Auto-generated Insurance Deductions
        if (bhxhAmount > 0)
        {
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "Deduction",
                Code = "DEDUCTION_BHXH",
                Name = "Bảo hiểm xã hội (8%)",
                Amount = bhxhAmount,
                SourceType = "Insurance",
                CreatedAt = DateTime.UtcNow
            });
        }
        if (bhytAmount > 0)
        {
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "Deduction",
                Code = "DEDUCTION_BHYT",
                Name = "Bảo hiểm y tế (1.5%)",
                Amount = bhytAmount,
                SourceType = "Insurance",
                CreatedAt = DateTime.UtcNow
            });
        }
        if (bhtnAmount > 0)
        {
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "Deduction",
                Code = "DEDUCTION_BHTN",
                Name = "Bảo hiểm thất nghiệp (1%)",
                Amount = bhtnAmount,
                SourceType = "Insurance",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Auto-generated PIT Deduction
        if (pitAmount > 0)
        {
            payslip.Items.Add(new PayslipItem
            {
                Id = Guid.NewGuid(),
                PayslipId = payslip.Id,
                ItemType = "Deduction",
                Code = "DEDUCTION_PIT",
                Name = "Thuế thu nhập cá nhân (TNCN)",
                Amount = pitAmount,
                SourceType = "Tax",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();

        var resultDto = new PayslipDto(
            payslip.Id,
            payslip.PayrollPeriodId,
            payslip.EmployeeId,
            payslip.Employee != null ? payslip.Employee.EmployeeCode : string.Empty,
            payslip.Employee != null ? payslip.Employee.FullName : string.Empty,
            payslip.BaseSalary,
            payslip.WorkedDays,
            payslip.PaidLeaveDays,
            payslip.GrossSalary,
            payslip.TotalDeduction,
            payslip.NetSalary,
            payslip.Status,
            payslip.Items.Select(i => new PayslipItemDto(i.Id, i.ItemType, i.Code, i.Name, i.Amount, i.SourceType)).ToList(),
            payslip.PayrollPeriod != null ? payslip.PayrollPeriod.Name : string.Empty,
            payslip.PayrollPeriod != null ? payslip.PayrollPeriod.Code : string.Empty
        );

        return Result<PayslipDto>.Success(resultDto, "Successfully updated payslip.");
    }

    private decimal CalculatePIT(decimal assessableIncome)
    {
        if (assessableIncome <= 0) return 0;

        decimal tax = 0;
        if (assessableIncome <= 5000000m)
        {
            tax = assessableIncome * 0.05m;
        }
        else if (assessableIncome <= 10000000m)
        {
            tax = (5000000m * 0.05m) + ((assessableIncome - 5000000m) * 0.10m);
        }
        else if (assessableIncome <= 18000000m)
        {
            tax = (5000000m * 0.05m) + (5000000m * 0.10m) + ((assessableIncome - 10000000m) * 0.15m);
        }
        else if (assessableIncome <= 32000000m)
        {
            tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + ((assessableIncome - 18000000m) * 0.20m);
        }
        else if (assessableIncome <= 52000000m)
        {
            tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + (14000000m * 0.20m) + ((assessableIncome - 32000000m) * 0.25m);
        }
        else if (assessableIncome <= 80000000m)
        {
            tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + (14000000m * 0.20m) + (20000000m * 0.25m) + ((assessableIncome - 52000000m) * 0.30m);
        }
        else
        {
            tax = (5000000m * 0.05m) + (5000000m * 0.10m) + (8000000m * 0.15m) + (14000000m * 0.20m) + (20000000m * 0.25m) + (28000000m * 0.30m) + ((assessableIncome - 80000000m) * 0.35m);
        }

        return Math.Round(tax, 0);
    }
}
