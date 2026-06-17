using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;

namespace Hrms.Attendance.Application.Interfaces;

public interface ILeaveRequestService
{
    Task<Result<IEnumerable<LeaveTypeDto>>> GetLeaveTypesAsync();
    Task<Result<IEnumerable<LeaveRequestDto>>> GetPersonalRequestsAsync(Guid employeeId);
    Task<Result<IEnumerable<LeaveRequestDto>>> GetRequestsAsync(string? status, Guid? departmentId, DateOnly? fromDate, DateOnly? toDate);
    Task<Result<LeaveRequestDto>> CreateAsync(Guid employeeId, CreateLeaveRequestDto dto);
    Task<Result<LeaveRequestDto>> GetByIdAsync(Guid id);
    Task<Result<LeaveRequestDto>> ApproveAsync(Guid id, Guid approvedByEmployeeId);
    Task<Result<LeaveRequestDto>> RejectAsync(Guid id, Guid approvedByEmployeeId);
    Task<Result<LeaveRequestDto>> CancelAsync(Guid id, Guid employeeId);
}
