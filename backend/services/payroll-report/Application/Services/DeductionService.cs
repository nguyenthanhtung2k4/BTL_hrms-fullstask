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

public class DeductionService : IDeductionService
{
    private readonly PayrollReportDbContext _dbContext;

    public DeductionService(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<EmployeeDeductionDto>>> GetDeductionsAsync(Guid? employeeId, Guid? periodId)
    {
        var query = _dbContext.EmployeeDeductions
            .Include(d => d.Employee)
            .Include(d => d.DeductionType)
            .Include(d => d.PayrollPeriod)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(d => d.EmployeeId == employeeId.Value);
        }

        if (periodId.HasValue)
        {
            query = query.Where(d => d.PayrollPeriodId == periodId.Value);
        }

        var list = await query
            .Select(d => new EmployeeDeductionDto(
                d.Id,
                d.EmployeeId,
                d.Employee != null ? d.Employee.EmployeeCode : string.Empty,
                d.Employee != null ? d.Employee.FullName : string.Empty,
                d.PayrollPeriodId,
                d.PayrollPeriod != null ? d.PayrollPeriod.Name : string.Empty,
                d.DeductionTypeId,
                d.DeductionType != null ? d.DeductionType.Code : string.Empty,
                d.DeductionType != null ? d.DeductionType.Name : string.Empty,
                d.Amount,
                d.Note,
                d.CreatedAt
            ))
            .ToListAsync();

        return Result<IEnumerable<EmployeeDeductionDto>>.Success(list, "Successfully retrieved employee deductions.");
    }

    public async Task<Result<EmployeeDeductionDto>> GetByIdAsync(Guid id)
    {
        var deduction = await _dbContext.EmployeeDeductions
            .Include(d => d.Employee)
            .Include(d => d.DeductionType)
            .Include(d => d.PayrollPeriod)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deduction == null)
        {
            return Result<EmployeeDeductionDto>.Failure("DeductionNotFound", "Employee deduction not found.");
        }

        var dto = new EmployeeDeductionDto(
            deduction.Id,
            deduction.EmployeeId,
            deduction.Employee != null ? deduction.Employee.EmployeeCode : string.Empty,
            deduction.Employee != null ? deduction.Employee.FullName : string.Empty,
            deduction.PayrollPeriodId,
            deduction.PayrollPeriod != null ? deduction.PayrollPeriod.Name : string.Empty,
            deduction.DeductionTypeId,
            deduction.DeductionType != null ? deduction.DeductionType.Code : string.Empty,
            deduction.DeductionType != null ? deduction.DeductionType.Name : string.Empty,
            deduction.Amount,
            deduction.Note,
            deduction.CreatedAt
        );

        return Result<EmployeeDeductionDto>.Success(dto, "Successfully retrieved employee deduction.");
    }

    public async Task<Result<EmployeeDeductionDto>> CreateAsync(CreateEmployeeDeductionDto dto)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(dto.PayrollPeriodId);
        if (period == null)
        {
            return Result<EmployeeDeductionDto>.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (period.Status == "Closed")
        {
            return Result<EmployeeDeductionDto>.Failure("PeriodClosed", "Cannot add deductions to a closed payroll period.");
        }

        var employeeExists = await _dbContext.EmployeeProjections.AnyAsync(e => e.Id == dto.EmployeeId);
        if (!employeeExists)
        {
            return Result<EmployeeDeductionDto>.Failure("EmployeeNotFound", "Employee not found.");
        }

        var type = await _dbContext.DeductionTypes.FindAsync(dto.DeductionTypeId);
        if (type == null)
        {
            return Result<EmployeeDeductionDto>.Failure("DeductionTypeNotFound", "Deduction type not found.");
        }

        var deduction = new EmployeeDeduction
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            PayrollPeriodId = dto.PayrollPeriodId,
            DeductionTypeId = dto.DeductionTypeId,
            Amount = dto.Amount,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.EmployeeDeductions.Add(deduction);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(deduction.Id);
    }

    public async Task<Result<EmployeeDeductionDto>> UpdateAsync(Guid id, UpdateEmployeeDeductionDto dto)
    {
        var deduction = await _dbContext.EmployeeDeductions
            .Include(d => d.PayrollPeriod)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deduction == null)
        {
            return Result<EmployeeDeductionDto>.Failure("DeductionNotFound", "Employee deduction not found.");
        }

        if (deduction.PayrollPeriod != null && deduction.PayrollPeriod.Status == "Closed")
        {
            return Result<EmployeeDeductionDto>.Failure("PeriodClosed", "Cannot update deductions in a closed payroll period.");
        }

        deduction.Amount = dto.Amount;
        deduction.Note = dto.Note;

        _dbContext.EmployeeDeductions.Update(deduction);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(deduction.Id);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var deduction = await _dbContext.EmployeeDeductions
            .Include(d => d.PayrollPeriod)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deduction == null)
        {
            return Result.Failure("DeductionNotFound", "Employee deduction not found.");
        }

        if (deduction.PayrollPeriod != null && deduction.PayrollPeriod.Status == "Closed")
        {
            return Result.Failure("PeriodClosed", "Cannot delete deductions from a closed payroll period.");
        }

        _dbContext.EmployeeDeductions.Remove(deduction);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Successfully deleted employee deduction.");
    }

    public async Task<Result<IEnumerable<DeductionTypeDto>>> GetDeductionTypesAsync()
    {
        var types = await _dbContext.DeductionTypes
            .Select(t => new DeductionTypeDto(t.Id, t.Code, t.Name, t.IsActive))
            .ToListAsync();

        return Result<IEnumerable<DeductionTypeDto>>.Success(types, "Successfully retrieved deduction types.");
    }
}
