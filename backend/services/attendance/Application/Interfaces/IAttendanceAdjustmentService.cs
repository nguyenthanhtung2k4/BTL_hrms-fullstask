using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;

namespace Hrms.Attendance.Application.Interfaces;

public interface IAttendanceAdjustmentService
{
    Task<Result<AttendanceAdjustmentDto>> CreateAsync(Guid employeeId, CreateAdjustmentRequest request);
    Task<Result<IEnumerable<AttendanceAdjustmentDto>>> GetPersonalRequestsAsync(Guid employeeId);
    Task<Result<IEnumerable<AttendanceAdjustmentDto>>> GetRequestsAsync(Guid? departmentId, Guid? employeeId, string? status = null);
    Task<Result<AttendanceAdjustmentDto>> ApproveAsync(Guid id, Guid handledByEmployeeId);
    Task<Result<AttendanceAdjustmentDto>> RejectAsync(Guid id, Guid handledByEmployeeId);
}
