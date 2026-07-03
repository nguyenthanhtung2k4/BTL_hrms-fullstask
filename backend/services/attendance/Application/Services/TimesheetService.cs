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

public class TimesheetService : ITimesheetService
{
    private readonly AttendanceDbContext _dbContext;

    public TimesheetService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<TimesheetDto>>> GetPersonalTimesheetsAsync(Guid employeeId, int? year)
    {
        var query = _dbContext.Timesheets
            .Include(t => t.Employee)
            .Where(t => t.EmployeeId == employeeId);

        if (year.HasValue)
        {
            query = query.Where(t => t.Year == year.Value);
        }

        var timesheets = await query.ToListAsync();
        var dtos = timesheets.Select(MapToDto);
        return Result<IEnumerable<TimesheetDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<TimesheetDto>>> GetTimesheetsAsync(int year, int month, Guid? departmentId, Guid? employeeId)
    {
        var query = _dbContext.Timesheets
            .Include(t => t.Employee)
            .Where(t => t.Year == year && t.Month == month);

        if (departmentId.HasValue)
        {
            query = query.Where(t => t.Employee.DepartmentId == departmentId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(t => t.EmployeeId == employeeId.Value);
        }

        var timesheets = await query.ToListAsync();
        var dtos = timesheets.Select(MapToDto);
        return Result<IEnumerable<TimesheetDto>>.Success(dtos);
    }

    public async Task<Result> RecalculateTimesheetsAsync(int year, int month)
    {
        var employees = await _dbContext.EmployeeProjections.Where(e => e.Status == "Active").ToListAsync();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var startOfMonth = new DateOnly(year, month, 1);
        var endOfMonth = new DateOnly(year, month, daysInMonth);

        foreach (var employee in employees)
        {
            // 1. Get all completed attendance records for this month
            var records = await _dbContext.AttendanceRecords
                .Where(r => r.EmployeeId == employee.Id && r.WorkDate >= startOfMonth && r.WorkDate <= endOfMonth && 
                       (r.Status == "Completed" || r.Status == "CheckedOut" || r.Status == "Late"))
                .ToListAsync();

            var totalWorkedMinutes = records.Sum(r => r.WorkedMinutes);

            // 2. Calculate leave days day-by-day to handle overlapping requests correctly
            decimal paidLeaveDays = 0;
            decimal unpaidLeaveDays = 0;

            var approvedLeaves = await _dbContext.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employee.Id && l.Status == "Approved" && l.FromDate <= endOfMonth && l.ToDate >= startOfMonth)
                .ToListAsync();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(year, month, day);
                var matchingLeave = approvedLeaves.FirstOrDefault(l => date >= l.FromDate && date <= l.ToDate);
                if (matchingLeave != null)
                {
                    if (matchingLeave.LeaveType.IsPaid)
                    {
                        paidLeaveDays += 1;
                    }
                    else
                    {
                        unpaidLeaveDays += 1;
                    }
                }
            }

            // 3. Save or update Timesheet
            var timesheet = await _dbContext.Timesheets
                .FirstOrDefaultAsync(t => t.EmployeeId == employee.Id && t.Year == year && t.Month == month);

            if (timesheet == null)
            {
                timesheet = new Timesheet
                {
                    EmployeeId = employee.Id,
                    Year = year,
                    Month = month,
                    TotalWorkedMinutes = totalWorkedMinutes,
                    PaidLeaveDays = paidLeaveDays,
                    UnpaidLeaveDays = unpaidLeaveDays,
                    Status = "Draft"
                };
                _dbContext.Timesheets.Add(timesheet);
            }
            else
            {
                timesheet.TotalWorkedMinutes = totalWorkedMinutes;
                timesheet.PaidLeaveDays = paidLeaveDays;
                timesheet.UnpaidLeaveDays = unpaidLeaveDays;
                timesheet.Status = "Draft";
                _dbContext.Timesheets.Update(timesheet);
            }
        }

        await _dbContext.SaveChangesAsync();
        return Result.Success("Timesheets recalculated successfully.");
    }

    private static TimesheetDto MapToDto(Timesheet t)
    {
        return new TimesheetDto(
            t.Id,
            t.EmployeeId,
            t.Employee.FullName,
            t.Year,
            t.Month,
            t.TotalWorkedMinutes,
            t.PaidLeaveDays,
            t.UnpaidLeaveDays,
            t.Status,
            t.CreatedAt,
            t.UpdatedAt
        );
    }
}
