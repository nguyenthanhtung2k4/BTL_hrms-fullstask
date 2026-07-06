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

public class AttendanceAdjustmentService : IAttendanceAdjustmentService
{
    private readonly AttendanceDbContext _dbContext;

    public AttendanceAdjustmentService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AttendanceAdjustmentDto>> CreateAsync(Guid employeeId, CreateAdjustmentRequest request)
    {
        var employee = await _dbContext.EmployeeProjections.FindAsync(employeeId);
        if (employee == null) return Result<AttendanceAdjustmentDto>.Failure("Employee not found.");

        var shift = await _dbContext.Shifts.FindAsync(request.ShiftId);
        if (shift == null) return Result<AttendanceAdjustmentDto>.Failure("Shift not found.");

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result<AttendanceAdjustmentDto>.Failure("Reason is required.");
        }

        var adjustment = new AttendanceAdjustment
        {
            EmployeeId = employeeId,
            WorkDate = request.WorkDate,
            ShiftId = request.ShiftId,
            ProposedCheckIn = request.ProposedCheckIn,
            ProposedCheckOut = request.ProposedCheckOut,
            Reason = request.Reason,
            Status = "Pending"
        };

        _dbContext.AttendanceAdjustments.Add(adjustment);
        await _dbContext.SaveChangesAsync();

        return await GetDtoByIdAsync(adjustment.Id);
    }

    public async Task<Result<IEnumerable<AttendanceAdjustmentDto>>> GetPersonalRequestsAsync(Guid employeeId)
    {
        var list = await _dbContext.AttendanceAdjustments
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Include(a => a.HandledBy)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<AttendanceAdjustmentDto>>.Success(list.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<AttendanceAdjustmentDto>>> GetRequestsAsync(Guid? departmentId, Guid? employeeId, string? status = null)
    {
        var query = _dbContext.AttendanceAdjustments
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Include(a => a.HandledBy)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(a => a.Employee.DepartmentId == departmentId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        var list = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return Result<IEnumerable<AttendanceAdjustmentDto>>.Success(list.Select(MapToDto));
    }

    public async Task<Result<AttendanceAdjustmentDto>> ApproveAsync(Guid id, Guid handledByEmployeeId)
    {
        var adjustment = await _dbContext.AttendanceAdjustments
            .Include(a => a.Shift)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (adjustment == null) return Result<AttendanceAdjustmentDto>.Failure("Adjustment request not found.");
        if (adjustment.Status != "Pending") return Result<AttendanceAdjustmentDto>.Failure("Request has already been processed.");

        // Handled status update
        adjustment.Status = "Approved";
        adjustment.HandledByEmployeeId = handledByEmployeeId;
        adjustment.HandledAt = DateTime.UtcNow;

        // Synchronize with AttendanceRecord
        var record = await _dbContext.AttendanceRecords
            .FirstOrDefaultAsync(r => r.EmployeeId == adjustment.EmployeeId && r.WorkDate == adjustment.WorkDate && r.ShiftId == adjustment.ShiftId);

        if (record == null)
        {
            // Create new record
            var schedule = await _dbContext.WorkSchedules
                .FirstOrDefaultAsync(s => s.EmployeeId == adjustment.EmployeeId && s.WorkDate == adjustment.WorkDate && s.ShiftId == adjustment.ShiftId);

            record = new AttendanceRecord
            {
                EmployeeId = adjustment.EmployeeId,
                WorkScheduleId = schedule?.Id,
                ShiftId = adjustment.ShiftId,
                WorkDate = adjustment.WorkDate,
                CheckInAt = adjustment.ProposedCheckIn ?? DateTime.UtcNow,
                CheckOutAt = adjustment.ProposedCheckOut,
                Status = adjustment.ProposedCheckOut.HasValue ? "Completed" : "CheckedIn",
                CheckInReason = $"[Giải trình duyệt] {adjustment.Reason}",
                CheckOutReason = adjustment.ProposedCheckOut.HasValue ? $"[Giải trình duyệt] {adjustment.Reason}" : null
            };

            if (adjustment.ProposedCheckIn.HasValue && adjustment.ProposedCheckOut.HasValue)
            {
                var totalMin = (int)(adjustment.ProposedCheckOut.Value - adjustment.ProposedCheckIn.Value).TotalMinutes;
                var workedMin = totalMin - adjustment.Shift.BreakMinutes;
                record.WorkedMinutes = workedMin > 0 ? workedMin : 0;
            }

            _dbContext.AttendanceRecords.Add(record);
        }
        else
        {
            // Update existing record
            if (adjustment.ProposedCheckIn.HasValue)
            {
                record.CheckInAt = adjustment.ProposedCheckIn.Value;
                record.CheckInReason = $"[Giải trình duyệt] {adjustment.Reason}";
            }
            if (adjustment.ProposedCheckOut.HasValue)
            {
                record.CheckOutAt = adjustment.ProposedCheckOut.Value;
                record.CheckOutReason = $"[Giải trình duyệt] {adjustment.Reason}";
                record.Status = "Completed";
            }

            if (record.CheckOutAt.HasValue)
            {
                var totalMin = (int)(record.CheckOutAt.Value - record.CheckInAt).TotalMinutes;
                var workedMin = totalMin - adjustment.Shift.BreakMinutes;
                record.WorkedMinutes = workedMin > 0 ? workedMin : 0;
            }
        }

        await _dbContext.SaveChangesAsync();
        return await GetDtoByIdAsync(adjustment.Id);
    }

    public async Task<Result<AttendanceAdjustmentDto>> RejectAsync(Guid id, Guid handledByEmployeeId)
    {
        var adjustment = await _dbContext.AttendanceAdjustments.FindAsync(id);
        if (adjustment == null) return Result<AttendanceAdjustmentDto>.Failure("Adjustment request not found.");
        if (adjustment.Status != "Pending") return Result<AttendanceAdjustmentDto>.Failure("Request has already been processed.");

        adjustment.Status = "Rejected";
        adjustment.HandledByEmployeeId = handledByEmployeeId;
        adjustment.HandledAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return await GetDtoByIdAsync(adjustment.Id);
    }

    private async Task<Result<AttendanceAdjustmentDto>> GetDtoByIdAsync(Guid id)
    {
        var a = await _dbContext.AttendanceAdjustments
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Include(a => a.HandledBy)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (a == null) return Result<AttendanceAdjustmentDto>.Failure("Adjustment not found.");
        return Result<AttendanceAdjustmentDto>.Success(MapToDto(a));
    }

    private static AttendanceAdjustmentDto MapToDto(AttendanceAdjustment a)
    {
        return new AttendanceAdjustmentDto(
            a.Id,
            a.EmployeeId,
            a.Employee?.FullName ?? "Unknown",
            a.WorkDate,
            a.ShiftId,
            a.Shift?.Name ?? "Unknown",
            a.ProposedCheckIn.HasValue ? DateTime.SpecifyKind(a.ProposedCheckIn.Value, DateTimeKind.Utc) : null,
            a.ProposedCheckOut.HasValue ? DateTime.SpecifyKind(a.ProposedCheckOut.Value, DateTimeKind.Utc) : null,
            a.Reason,
            a.Status,
            a.HandledByEmployeeId,
            a.HandledBy?.FullName,
            a.HandledAt.HasValue ? DateTime.SpecifyKind(a.HandledAt.Value, DateTimeKind.Utc) : null,
            DateTime.SpecifyKind(a.CreatedAt, DateTimeKind.Utc)
        );
    }
}
