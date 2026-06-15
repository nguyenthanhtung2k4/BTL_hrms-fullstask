using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Attendance.Domain.Entities;
using Hrms.Attendance.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hrms.Attendance.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AttendanceService(AttendanceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<AttendanceRecordDto>> CheckInAsync(Guid employeeId, string shiftCode)
    {
        // 1. Validate employee
        var employee = await _dbContext.EmployeeProjections.FindAsync(employeeId);
        if (employee == null) return Result<AttendanceRecordDto>.Failure("Employee not found.");

        if (employee.Status != "Active")
        {
            return Result<AttendanceRecordDto>.Failure("Cannot check-in. Employee status is not Active.");
        }

        // 2. Validate shift
        var shift = await _dbContext.Shifts.FirstOrDefaultAsync(s => s.Code == shiftCode && s.IsActive);
        if (shift == null) return Result<AttendanceRecordDto>.Failure($"Shift with code '{shiftCode}' not found or inactive.");

        var today = DateOnly.FromDateTime(DateTime.Today);

        // 3. Check duplicate check-in
        var existingRecord = await _dbContext.AttendanceRecords
            .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && r.WorkDate == today && r.ShiftId == shift.Id);

        if (existingRecord != null)
        {
            return Result<AttendanceRecordDto>.Failure("Employee has already checked-in for this shift today.");
        }

        // 4. Find work schedule if any
        var workSchedule = await _dbContext.WorkSchedules
            .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.WorkDate == today && w.ShiftId == shift.Id);

        // 5. Save attendance record
        var record = new AttendanceRecord
        {
            EmployeeId = employeeId,
            WorkScheduleId = workSchedule?.Id,
            ShiftId = shift.Id,
            WorkDate = today,
            CheckInAt = DateTime.UtcNow,
            CheckOutAt = null,
            WorkedMinutes = 0,
            Status = "CheckedIn"
        };

        _dbContext.AttendanceRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // 6. Publish event
        var integrationEvent = new IntegrationEvent<AttendanceRecordedPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.AttendanceRecorded,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "attendance",
            CorrelationId: null,
            Payload: new AttendanceRecordedPayload(
                record.Id,
                record.EmployeeId,
                record.WorkDate,
                record.ShiftId,
                record.CheckInAt,
                record.CheckOutAt,
                record.WorkedMinutes,
                record.Status
            )
        );
        await _publishEndpoint.Publish(integrationEvent);

        return await GetRecordDtoByIdAsync(record.Id);
    }

    public async Task<Result<AttendanceRecordDto>> CheckOutAsync(Guid employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Find CheckedIn record for today
        var record = await _dbContext.AttendanceRecords
            .Include(r => r.Shift)
            .Where(r => r.EmployeeId == employeeId && r.WorkDate == today && r.Status == "CheckedIn")
            .OrderByDescending(r => r.CheckInAt)
            .FirstOrDefaultAsync();

        if (record == null)
        {
            // Try yesterday's check-in in case it was overnight
            var yesterday = today.AddDays(-1);
            record = await _dbContext.AttendanceRecords
                .Include(r => r.Shift)
                .Where(r => r.EmployeeId == employeeId && r.WorkDate == yesterday && r.Status == "CheckedIn" && r.Shift.IsOvernight)
                .OrderByDescending(r => r.CheckInAt)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                return Result<AttendanceRecordDto>.Failure("No check-in record found for this employee to check-out.");
            }
        }

        var checkOutTime = DateTime.UtcNow;

        if (checkOutTime <= record.CheckInAt)
        {
            return Result<AttendanceRecordDto>.Failure("Check-out time must be after check-in time.");
        }

        // Calculate worked minutes
        var totalMinutes = (int)(checkOutTime - record.CheckInAt).TotalMinutes;
        var breakMinutes = record.Shift.BreakMinutes;
        var workedMinutes = totalMinutes - breakMinutes;
        if (workedMinutes < 0) workedMinutes = 0;

        record.CheckOutAt = checkOutTime;
        record.WorkedMinutes = workedMinutes;
        record.Status = "Completed";

        await _dbContext.SaveChangesAsync();

        // Publish event
        var integrationEvent = new IntegrationEvent<AttendanceRecordedPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.AttendanceRecorded,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "attendance",
            CorrelationId: null,
            Payload: new AttendanceRecordedPayload(
                record.Id,
                record.EmployeeId,
                record.WorkDate,
                record.ShiftId,
                record.CheckInAt,
                record.CheckOutAt,
                record.WorkedMinutes,
                record.Status
            )
        );
        await _publishEndpoint.Publish(integrationEvent);

        return await GetRecordDtoByIdAsync(record.Id);
    }

    public async Task<Result<IEnumerable<AttendanceRecordDto>>> GetPersonalRecordsAsync(Guid employeeId, DateOnly? fromDate, DateOnly? toDate)
    {
        return await GetRecordsAsync(employeeId, null, fromDate, toDate);
    }

    public async Task<Result<IEnumerable<AttendanceRecordDto>>> GetRecordsAsync(Guid? employeeId, Guid? departmentId, DateOnly? fromDate, DateOnly? toDate)
    {
        var query = _dbContext.AttendanceRecords
            .Include(r => r.Employee)
            .Include(r => r.Shift)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(r => r.Employee.DepartmentId == departmentId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.WorkDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.WorkDate <= toDate.Value);
        }

        var records = await query.ToListAsync();
        var dtos = records.Select(MapToDto);
        return Result<IEnumerable<AttendanceRecordDto>>.Success(dtos);
    }

    private async Task<Result<AttendanceRecordDto>> GetRecordDtoByIdAsync(Guid id)
    {
        var r = await _dbContext.AttendanceRecords
            .Include(r => r.Employee)
            .Include(r => r.Shift)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return Result<AttendanceRecordDto>.Failure("Record not found.");
        return Result<AttendanceRecordDto>.Success(MapToDto(r));
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord r)
    {
        return new AttendanceRecordDto(
            r.Id,
            r.EmployeeId,
            r.Employee.FullName,
            r.WorkScheduleId,
            r.ShiftId,
            r.Shift.Name,
            r.WorkDate,
            r.CheckInAt,
            r.CheckOutAt,
            r.WorkedMinutes,
            r.Status,
            r.CreatedAt,
            r.UpdatedAt
        );
    }
}
