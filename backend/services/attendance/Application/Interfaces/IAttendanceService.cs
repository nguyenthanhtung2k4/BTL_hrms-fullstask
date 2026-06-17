using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;

namespace Hrms.Attendance.Application.Interfaces;

public interface IAttendanceService
{
    Task<Result<AttendanceRecordDto>> CheckInAsync(Guid employeeId, string shiftCode);
    Task<Result<AttendanceRecordDto>> CheckOutAsync(Guid employeeId);
    Task<Result<IEnumerable<AttendanceRecordDto>>> GetPersonalRecordsAsync(Guid employeeId, DateOnly? fromDate, DateOnly? toDate);
    Task<Result<IEnumerable<AttendanceRecordDto>>> GetRecordsAsync(Guid? employeeId, Guid? departmentId, DateOnly? fromDate, DateOnly? toDate);
}
