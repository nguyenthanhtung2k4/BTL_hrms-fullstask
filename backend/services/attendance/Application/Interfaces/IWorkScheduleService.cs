using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;

namespace Hrms.Attendance.Application.Interfaces;

public interface IWorkScheduleService
{
    Task<Result<IEnumerable<WorkScheduleDto>>> GetSchedulesAsync(Guid? employeeId, DateOnly? fromDate, DateOnly? toDate, List<Guid>? allowedEmployeeIds = null);
    Task<Result<WorkScheduleDto>> GetByIdAsync(Guid id);
    Task<Result<WorkScheduleDto>> CreateAsync(CreateWorkScheduleDto dto);
    Task<Result<WorkScheduleDto>> UpdateAsync(Guid id, UpdateWorkScheduleDto dto);
    Task<Result> DeleteAsync(Guid id);
}
