using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.PayrollReport.Domain.Entities;
using Hrms.PayrollReport.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hrms.PayrollReport.Application.Services;

public class PayrollPeriodService : IPayrollPeriodService
{
    private readonly PayrollReportDbContext _dbContext;
    private readonly IPayslipService _payslipService;
    private readonly IPublishEndpoint _publishEndpoint;

    public PayrollPeriodService(
        PayrollReportDbContext dbContext,
        IPayslipService payslipService,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _payslipService = payslipService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<IEnumerable<PayrollPeriodDto>>> GetAllAsync()
    {
        var periods = await _dbContext.PayrollPeriods
            .OrderByDescending(p => p.FromDate)
            .Select(p => new PayrollPeriodDto(
                p.Id,
                p.Code,
                p.Name,
                p.FromDate,
                p.ToDate,
                p.StandardWorkDays,
                p.PayrollRuleId,
                p.Status,
                p.ClosedAt,
                p.CreatedAt
            ))
            .ToListAsync();

        return Result<IEnumerable<PayrollPeriodDto>>.Success(periods, "Successfully retrieved all payroll periods.");
    }

    public async Task<Result<PayrollPeriodDto>> GetByIdAsync(Guid id)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(id);
        if (period == null)
        {
            return Result<PayrollPeriodDto>.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        var dto = new PayrollPeriodDto(
            period.Id,
            period.Code,
            period.Name,
            period.FromDate,
            period.ToDate,
            period.StandardWorkDays,
            period.PayrollRuleId,
            period.Status,
            period.ClosedAt,
            period.CreatedAt
        );

        return Result<PayrollPeriodDto>.Success(dto, "Successfully retrieved payroll period.");
    }

    public async Task<Result<PayrollPeriodDto>> CreateAsync(CreatePayrollPeriodDto dto)
    {
        var exists = await _dbContext.PayrollPeriods.AnyAsync(p => p.Code == dto.Code);
        if (exists)
        {
            return Result<PayrollPeriodDto>.Failure("DuplicatePeriodCode", $"Payroll period with code '{dto.Code}' already exists.");
        }

        var ruleExists = await _dbContext.PayrollRules.AnyAsync(r => r.Id == dto.PayrollRuleId);
        if (!ruleExists)
        {
            return Result<PayrollPeriodDto>.Failure("PayrollRuleNotFound", "Payroll rule not found.");
        }

        var period = new PayrollPeriod
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            StandardWorkDays = dto.StandardWorkDays,
            PayrollRuleId = dto.PayrollRuleId,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PayrollPeriods.Add(period);
        await _dbContext.SaveChangesAsync();

        var resultDto = new PayrollPeriodDto(
            period.Id,
            period.Code,
            period.Name,
            period.FromDate,
            period.ToDate,
            period.StandardWorkDays,
            period.PayrollRuleId,
            period.Status,
            period.ClosedAt,
            period.CreatedAt
        );

        return Result<PayrollPeriodDto>.Success(resultDto, "Successfully created payroll period.");
    }

    public async Task<Result<PayrollPeriodDto>> UpdateAsync(Guid id, UpdatePayrollPeriodDto dto)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(id);
        if (period == null)
        {
            return Result<PayrollPeriodDto>.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (period.Status == "Closed")
        {
            return Result<PayrollPeriodDto>.Failure("PeriodClosed", "Cannot update a closed payroll period.");
        }

        var ruleExists = await _dbContext.PayrollRules.AnyAsync(r => r.Id == dto.PayrollRuleId);
        if (!ruleExists)
        {
            return Result<PayrollPeriodDto>.Failure("PayrollRuleNotFound", "Payroll rule not found.");
        }

        // ĐÃ SỬA: Cập nhật mã Code từ DTO để sửa lỗi không lưu được mã kỳ lương
        period.Code = dto.Code;

        period.Name = dto.Name;
        period.FromDate = dto.FromDate;
        period.ToDate = dto.ToDate;
        period.StandardWorkDays = dto.StandardWorkDays;
        period.PayrollRuleId = dto.PayrollRuleId;
        period.UpdatedAt = DateTime.UtcNow;

        _dbContext.PayrollPeriods.Update(period);
        await _dbContext.SaveChangesAsync();

        var resultDto = new PayrollPeriodDto(
            period.Id,
            period.Code,
            period.Name,
            period.FromDate,
            period.ToDate,
            period.StandardWorkDays,
            period.PayrollRuleId,
            period.Status,
            period.ClosedAt,
            period.CreatedAt
        );

        return Result<PayrollPeriodDto>.Success(resultDto, "Successfully updated payroll period.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(id);
        if (period == null)
        {
            return Result.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (period.Status == "Closed")
        {
            return Result.Failure("PeriodClosed", "Cannot delete a closed payroll period.");
        }

        var payslips = await _dbContext.Payslips.Where(p => p.PayrollPeriodId == id).ToListAsync();
        _dbContext.Payslips.RemoveRange(payslips);

        var allowances = await _dbContext.EmployeeAllowances.Where(a => a.PayrollPeriodId == id).ToListAsync();
        _dbContext.EmployeeAllowances.RemoveRange(allowances);

        var deductions = await _dbContext.EmployeeDeductions.Where(d => d.PayrollPeriodId == id).ToListAsync();
        _dbContext.EmployeeDeductions.RemoveRange(deductions);

        _dbContext.PayrollPeriods.Remove(period);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Successfully deleted payroll period.");
    }

    public async Task<Result> CalculateAsync(Guid id)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(id);
        if (period == null)
        {
            return Result.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (period.Status == "Closed")
        {
            return Result.Failure("PeriodClosed", "Cannot recalculate payroll for a closed payroll period.");
        }

        return await _payslipService.CalculatePeriodPayslipsAsync(id);
    }

    public async Task<Result> CloseAsync(Guid id, Guid closedBy)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(id);
        if (period == null)
        {
            return Result.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (period.Status == "Closed")
        {
            return Result.Failure("PeriodAlreadyClosed", "Payroll period is already closed.");
        }

        period.Status = "Closed";
        period.ClosedAt = DateTime.UtcNow;
        period.UpdatedAt = DateTime.UtcNow;

        _dbContext.PayrollPeriods.Update(period);

        var payslips = await _dbContext.Payslips.Where(p => p.PayrollPeriodId == id).ToListAsync();
        foreach (var payslip in payslips)
        {
            payslip.Status = "Closed";
            payslip.UpdatedAt = DateTime.UtcNow;
            _dbContext.Payslips.Update(payslip);
        }

        await _dbContext.SaveChangesAsync();

        var payload = new PayrollClosedPayload(
            PayrollPeriodId: period.Id,
            PeriodName: period.Name,
            FromDate: period.FromDate,
            ToDate: period.ToDate,
            ClosedAt: DateTimeOffset.UtcNow,
            ClosedBy: closedBy
        );

        var integrationEvent = new IntegrationEvent<PayrollClosedPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.PayrollClosed,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "payroll-report",
            CorrelationId: null,
            Payload: payload
        );

        await _publishEndpoint.Publish(integrationEvent);

        return Result.Success("Successfully closed payroll period.");
    }
}