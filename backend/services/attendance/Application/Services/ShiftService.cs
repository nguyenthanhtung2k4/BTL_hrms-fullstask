using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Attendance.Domain.Entities;
using Hrms.Attendance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hrms.Attendance.Application.Services;

public class ShiftService : IShiftService
{
    private readonly AttendanceDbContext _dbContext;

    public ShiftService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<ShiftDto>>> GetAllAsync()
    {
        var shifts = await _dbContext.Shifts.ToListAsync();
        var dtos = shifts.Select(MapToDto);
        return Result<IEnumerable<ShiftDto>>.Success(dtos);
    }

    public async Task<Result<ShiftDto>> GetByIdAsync(Guid id)
    {
        var shift = await _dbContext.Shifts.FindAsync(id);
        if (shift == null) return Result<ShiftDto>.Failure("Shift not found.");
        return Result<ShiftDto>.Success(MapToDto(shift));
    }

    public async Task<Result<ShiftDto>> CreateAsync(CreateShiftDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<ShiftDto>.Failure("Code and Name are required.");
        }

        if (await _dbContext.Shifts.AnyAsync(s => s.Code == dto.Code))
        {
            return Result<ShiftDto>.Failure($"Shift with code '{dto.Code}' already exists.");
        }

        var shift = new Shift
        {
            Code = dto.Code,
            Name = dto.Name,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            BreakMinutes = dto.BreakMinutes,
            IsOvernight = dto.IsOvernight,
            IsActive = true
        };

        _dbContext.Shifts.Add(shift);
        await _dbContext.SaveChangesAsync();

        return Result<ShiftDto>.Success(MapToDto(shift));
    }

    public async Task<Result<ShiftDto>> UpdateAsync(Guid id, UpdateShiftDto dto)
    {
        var shift = await _dbContext.Shifts.FindAsync(id);
        if (shift == null) return Result<ShiftDto>.Failure("Shift not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<ShiftDto>.Failure("Name is required.");
        }

        shift.Name = dto.Name;
        shift.StartTime = dto.StartTime;
        shift.EndTime = dto.EndTime;
        shift.BreakMinutes = dto.BreakMinutes;
        shift.IsOvernight = dto.IsOvernight;
        shift.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();

        return Result<ShiftDto>.Success(MapToDto(shift));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var shift = await _dbContext.Shifts.FindAsync(id);
        if (shift == null) return Result.Failure("Shift not found.");

        if (await _dbContext.WorkSchedules.AnyAsync(w => w.ShiftId == id))
        {
            return Result.Failure("Cannot delete shift because it is assigned in work schedules.");
        }

        _dbContext.Shifts.Remove(shift);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Shift deleted successfully.");
    }

    private static ShiftDto MapToDto(Shift s)
    {
        return new ShiftDto(
            s.Id,
            s.Code,
            s.Name,
            s.StartTime,
            s.EndTime,
            s.BreakMinutes,
            s.IsOvernight,
            s.IsActive,
            s.CreatedAt,
            s.UpdatedAt
        );
    }
}
