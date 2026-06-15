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

public class WorkScheduleService : IWorkScheduleService
{
    private readonly AttendanceDbContext _dbContext;

    public WorkScheduleService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<WorkScheduleDto>>> GetSchedulesAsync(Guid? employeeId, DateOnly? fromDate, DateOnly? toDate)
    {
        var query = _dbContext.WorkSchedules
            .Include(w => w.Employee)
            .Include(w => w.Shift)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(w => w.EmployeeId == employeeId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(w => w.WorkDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(w => w.WorkDate <= toDate.Value);
        }

        var schedules = await query.ToListAsync();
        var dtos = schedules.Select(MapToDto);
        return Result<IEnumerable<WorkScheduleDto>>.Success(dtos);
    }

    public async Task<Result<WorkScheduleDto>> GetByIdAsync(Guid id)
    {
        var schedule = await _dbContext.WorkSchedules
            .Include(w => w.Employee)
            .Include(w => w.Shift)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (schedule == null) return Result<WorkScheduleDto>.Failure("Work schedule not found.");
        return Result<WorkScheduleDto>.Success(MapToDto(schedule));
    }

    public async Task<Result<WorkScheduleDto>> CreateAsync(CreateWorkScheduleDto dto)
    {
        var employee = await _dbContext.EmployeeProjections.FindAsync(dto.EmployeeId);
        if (employee == null) return Result<WorkScheduleDto>.Failure("Employee not found.");

        if (employee.Status != "Active")
        {
            return Result<WorkScheduleDto>.Failure("Cannot create work schedule for inactive employee.");
        }

        var shift = await _dbContext.Shifts.FindAsync(dto.ShiftId);
        if (shift == null) return Result<WorkScheduleDto>.Failure("Shift not found.");

        // Check if duplicate
        if (await _dbContext.WorkSchedules.AnyAsync(w => w.EmployeeId == dto.EmployeeId && w.WorkDate == dto.WorkDate && w.ShiftId == dto.ShiftId))
        {
            return Result<WorkScheduleDto>.Failure("This employee is already scheduled for this shift on the selected date.");
        }

        var schedule = new WorkSchedule
        {
            EmployeeId = dto.EmployeeId,
            ShiftId = dto.ShiftId,
            WorkDate = dto.WorkDate,
            Status = "Planned"
        };

        _dbContext.WorkSchedules.Add(schedule);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(schedule.Id);
    }

    public async Task<Result<WorkScheduleDto>> UpdateAsync(Guid id, UpdateWorkScheduleDto dto)
    {
        var schedule = await _dbContext.WorkSchedules.FindAsync(id);
        if (schedule == null) return Result<WorkScheduleDto>.Failure("Work schedule not found.");

        var shift = await _dbContext.Shifts.FindAsync(dto.ShiftId);
        if (shift == null) return Result<WorkScheduleDto>.Failure("Shift not found.");

        // Check duplicate if date/shift changed
        if (schedule.ShiftId != dto.ShiftId || schedule.WorkDate != dto.WorkDate)
        {
            if (await _dbContext.WorkSchedules.AnyAsync(w => w.EmployeeId == schedule.EmployeeId && w.WorkDate == dto.WorkDate && w.ShiftId == dto.ShiftId && w.Id != id))
            {
                return Result<WorkScheduleDto>.Failure("This employee is already scheduled for this shift on the selected date.");
            }
        }

        schedule.ShiftId = dto.ShiftId;
        schedule.WorkDate = dto.WorkDate;
        schedule.Status = dto.Status;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var schedule = await _dbContext.WorkSchedules.FindAsync(id);
        if (schedule == null) return Result.Failure("Work schedule not found.");

        _dbContext.WorkSchedules.Remove(schedule);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Work schedule deleted successfully.");
    }

    private static WorkScheduleDto MapToDto(WorkSchedule w)
    {
        return new WorkScheduleDto(
            w.Id,
            w.EmployeeId,
            w.Employee.FullName,
            w.ShiftId,
            w.Shift.Name,
            w.WorkDate,
            w.Status,
            w.CreatedAt,
            w.UpdatedAt
        );
    }
}
