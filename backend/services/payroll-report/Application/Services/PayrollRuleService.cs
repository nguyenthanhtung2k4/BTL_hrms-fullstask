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

public class PayrollRuleService : IPayrollRuleService
{
    private readonly PayrollReportDbContext _dbContext;

    public PayrollRuleService(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<PayrollRuleDto>>> GetAllAsync()
    {
        var rules = await _dbContext.PayrollRules
            .Select(r => new PayrollRuleDto(
                r.Id,
                r.Code,
                r.Name,
                r.WorkDayHours,
                r.PaidLeaveCountsAsWork,
                r.OvertimeRate,
                r.IsActive,
                r.GracePeriodMinutes,
                r.LateDeductionRate,
                r.WeekendOvertimeRate,
                r.HolidayOvertimeRate,
                r.RoundingMinutes
            ))
            .ToListAsync();

        return Result<IEnumerable<PayrollRuleDto>>.Success(rules, "Successfully retrieved all payroll rules.");
    }

    public async Task<Result<PayrollRuleDto>> GetByIdAsync(Guid id)
    {
        var rule = await _dbContext.PayrollRules.FindAsync(id);
        if (rule == null)
        {
            return Result<PayrollRuleDto>.Failure("PayrollRuleNotFound", "Payroll rule not found.");
        }

        var dto = new PayrollRuleDto(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.WorkDayHours,
            rule.PaidLeaveCountsAsWork,
            rule.OvertimeRate,
            rule.IsActive,
            rule.GracePeriodMinutes,
            rule.LateDeductionRate,
            rule.WeekendOvertimeRate,
            rule.HolidayOvertimeRate,
            rule.RoundingMinutes
        );

        return Result<PayrollRuleDto>.Success(dto, "Successfully retrieved payroll rule.");
    }

    public async Task<Result<PayrollRuleDto>> CreateAsync(CreatePayrollRuleDto dto)
    {
        var exists = await _dbContext.PayrollRules.AnyAsync(r => r.Code == dto.Code);
        if (exists)
        {
            return Result<PayrollRuleDto>.Failure("DuplicateRuleCode", $"Payroll rule with code '{dto.Code}' already exists.");
        }

        var rule = new PayrollRule
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            WorkDayHours = dto.WorkDayHours,
            PaidLeaveCountsAsWork = dto.PaidLeaveCountsAsWork,
            OvertimeRate = dto.OvertimeRate,
            IsActive = true,
            GracePeriodMinutes = dto.GracePeriodMinutes,
            LateDeductionRate = dto.LateDeductionRate,
            WeekendOvertimeRate = dto.WeekendOvertimeRate,
            HolidayOvertimeRate = dto.HolidayOvertimeRate,
            RoundingMinutes = dto.RoundingMinutes
        };

        _dbContext.PayrollRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var resultDto = new PayrollRuleDto(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.WorkDayHours,
            rule.PaidLeaveCountsAsWork,
            rule.OvertimeRate,
            rule.IsActive,
            rule.GracePeriodMinutes,
            rule.LateDeductionRate,
            rule.WeekendOvertimeRate,
            rule.HolidayOvertimeRate,
            rule.RoundingMinutes
        );

        return Result<PayrollRuleDto>.Success(resultDto, "Successfully created payroll rule.");
    }

    public async Task<Result<PayrollRuleDto>> UpdateAsync(Guid id, UpdatePayrollRuleDto dto)
    {
        var rule = await _dbContext.PayrollRules.FindAsync(id);
        if (rule == null)
        {
            return Result<PayrollRuleDto>.Failure("PayrollRuleNotFound", "Payroll rule not found.");
        }

        rule.Name = dto.Name;
        rule.WorkDayHours = dto.WorkDayHours;
        rule.PaidLeaveCountsAsWork = dto.PaidLeaveCountsAsWork;
        rule.OvertimeRate = dto.OvertimeRate;
        rule.IsActive = dto.IsActive;
        rule.GracePeriodMinutes = dto.GracePeriodMinutes;
        rule.LateDeductionRate = dto.LateDeductionRate;
        rule.WeekendOvertimeRate = dto.WeekendOvertimeRate;
        rule.HolidayOvertimeRate = dto.HolidayOvertimeRate;
        rule.RoundingMinutes = dto.RoundingMinutes;

        _dbContext.PayrollRules.Update(rule);
        await _dbContext.SaveChangesAsync();

        var resultDto = new PayrollRuleDto(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.WorkDayHours,
            rule.PaidLeaveCountsAsWork,
            rule.OvertimeRate,
            rule.IsActive,
            rule.GracePeriodMinutes,
            rule.LateDeductionRate,
            rule.WeekendOvertimeRate,
            rule.HolidayOvertimeRate,
            rule.RoundingMinutes
        );

        return Result<PayrollRuleDto>.Success(resultDto, "Successfully updated payroll rule.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var rule = await _dbContext.PayrollRules.FindAsync(id);
        if (rule == null)
        {
            return Result.Failure("PayrollRuleNotFound", "Payroll rule not found.");
        }

        // Check if there are periods referencing this rule
        var isReferenced = await _dbContext.PayrollPeriods.AnyAsync(p => p.PayrollRuleId == id);
        if (isReferenced)
        {
            return Result.Failure("RuleInUse", "Cannot delete payroll rule because it is currently assigned to one or more payroll periods.");
        }

        _dbContext.PayrollRules.Remove(rule);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Successfully deleted payroll rule.");
    }
}
