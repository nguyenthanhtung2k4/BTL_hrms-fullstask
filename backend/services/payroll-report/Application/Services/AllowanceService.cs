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

public class AllowanceService : IAllowanceService
{
    private readonly PayrollReportDbContext _dbContext;

    public AllowanceService(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<EmployeeAllowanceDto>>> GetAllowancesAsync(Guid? employeeId, Guid? periodId)
    {
        var query = _dbContext.EmployeeAllowances
            .Include(a => a.Employee)
            .Include(a => a.AllowanceType)
            .Include(a => a.PayrollPeriod)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (periodId.HasValue)
        {
            query = query.Where(a => a.PayrollPeriodId == periodId.Value);
        }

        var list = await query
            .Select(a => new EmployeeAllowanceDto(
                a.Id,
                a.EmployeeId,
                a.Employee != null ? a.Employee.EmployeeCode : string.Empty,
                a.Employee != null ? a.Employee.FullName : string.Empty,
                a.PayrollPeriodId,
                a.PayrollPeriod != null ? a.PayrollPeriod.Name : string.Empty,
                a.AllowanceTypeId,
                a.AllowanceType != null ? a.AllowanceType.Code : string.Empty,
                a.AllowanceType != null ? a.AllowanceType.Name : string.Empty,
                a.Amount,
                a.Note,
                a.CreatedAt
            ))
            .ToListAsync();

        return Result<IEnumerable<EmployeeAllowanceDto>>.Success(list, "Successfully retrieved employee allowances.");
    }

    public async Task<Result<EmployeeAllowanceDto>> GetByIdAsync(Guid id)
    {
        var allowance = await _dbContext.EmployeeAllowances
            .Include(a => a.Employee)
            .Include(a => a.AllowanceType)
            .Include(a => a.PayrollPeriod)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (allowance == null)
        {
            return Result<EmployeeAllowanceDto>.Failure("AllowanceNotFound", "Employee allowance not found.");
        }

        var dto = new EmployeeAllowanceDto(
            allowance.Id,
            allowance.EmployeeId,
            allowance.Employee != null ? allowance.Employee.EmployeeCode : string.Empty,
            allowance.Employee != null ? allowance.Employee.FullName : string.Empty,
            allowance.PayrollPeriodId,
            allowance.PayrollPeriod != null ? allowance.PayrollPeriod.Name : string.Empty,
            allowance.AllowanceTypeId,
            allowance.AllowanceType != null ? allowance.AllowanceType.Code : string.Empty,
            allowance.AllowanceType != null ? allowance.AllowanceType.Name : string.Empty,
            allowance.Amount,
            allowance.Note,
            allowance.CreatedAt
        );

        return Result<EmployeeAllowanceDto>.Success(dto, "Successfully retrieved employee allowance.");
    }

    public async Task<Result<EmployeeAllowanceDto>> CreateAsync(CreateEmployeeAllowanceDto dto)
    {
        var period = await _dbContext.PayrollPeriods.FindAsync(dto.PayrollPeriodId);
        if (period == null)
        {
            return Result<EmployeeAllowanceDto>.Failure("PayrollPeriodNotFound", "Payroll period not found.");
        }

        if (period.Status == "Closed")
        {
            return Result<EmployeeAllowanceDto>.Failure("PeriodClosed", "Cannot add allowances to a closed payroll period.");
        }

        var employeeExists = await _dbContext.EmployeeProjections.AnyAsync(e => e.Id == dto.EmployeeId);
        if (!employeeExists)
        {
            return Result<EmployeeAllowanceDto>.Failure("EmployeeNotFound", "Employee not found.");
        }

        var type = await _dbContext.AllowanceTypes.FindAsync(dto.AllowanceTypeId);
        if (type == null)
        {
            return Result<EmployeeAllowanceDto>.Failure("AllowanceTypeNotFound", "Allowance type not found.");
        }

        var allowance = new EmployeeAllowance
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            PayrollPeriodId = dto.PayrollPeriodId,
            AllowanceTypeId = dto.AllowanceTypeId,
            Amount = dto.Amount,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.EmployeeAllowances.Add(allowance);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(allowance.Id);
    }

    public async Task<Result<EmployeeAllowanceDto>> UpdateAsync(Guid id, UpdateEmployeeAllowanceDto dto)
    {
        var allowance = await _dbContext.EmployeeAllowances
            .Include(a => a.PayrollPeriod)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (allowance == null)
        {
            return Result<EmployeeAllowanceDto>.Failure("AllowanceNotFound", "Employee allowance not found.");
        }

        if (allowance.PayrollPeriod != null && allowance.PayrollPeriod.Status == "Closed")
        {
            return Result<EmployeeAllowanceDto>.Failure("PeriodClosed", "Cannot update allowances in a closed payroll period.");
        }

        allowance.Amount = dto.Amount;
        allowance.Note = dto.Note;

        _dbContext.EmployeeAllowances.Update(allowance);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(allowance.Id);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var allowance = await _dbContext.EmployeeAllowances
            .Include(a => a.PayrollPeriod)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (allowance == null)
        {
            return Result.Failure("AllowanceNotFound", "Employee allowance not found.");
        }

        if (allowance.PayrollPeriod != null && allowance.PayrollPeriod.Status == "Closed")
        {
            return Result.Failure("PeriodClosed", "Cannot delete allowances from a closed payroll period.");
        }

        _dbContext.EmployeeAllowances.Remove(allowance);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Successfully deleted employee allowance.");
    }

    public async Task<Result<IEnumerable<AllowanceTypeDto>>> GetAllowanceTypesAsync()
    {
        var types = await _dbContext.AllowanceTypes
            .Select(t => new AllowanceTypeDto(t.Id, t.Code, t.Name, t.IsActive))
            .ToListAsync();

        return Result<IEnumerable<AllowanceTypeDto>>.Success(types, "Successfully retrieved allowance types.");
    }
}
