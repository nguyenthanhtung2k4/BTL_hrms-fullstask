using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;

namespace Hrms.Attendance.Application.Interfaces;

public interface ITimesheetService
{
    Task<Result<IEnumerable<TimesheetDto>>> GetPersonalTimesheetsAsync(Guid employeeId, int? year);
    Task<Result<IEnumerable<TimesheetDto>>> GetTimesheetsAsync(int year, int month, Guid? departmentId, Guid? employeeId);
    Task<Result> RecalculateTimesheetsAsync(int year, int month);
}
