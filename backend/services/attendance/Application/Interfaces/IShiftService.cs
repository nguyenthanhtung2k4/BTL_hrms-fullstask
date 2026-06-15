using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;

namespace Hrms.Attendance.Application.Interfaces;

public interface IShiftService
{
    Task<Result<IEnumerable<ShiftDto>>> GetAllAsync();
    Task<Result<ShiftDto>> GetByIdAsync(Guid id);
    Task<Result<ShiftDto>> CreateAsync(CreateShiftDto dto);
    Task<Result<ShiftDto>> UpdateAsync(Guid id, UpdateShiftDto dto);
    Task<Result> DeleteAsync(Guid id);
}
