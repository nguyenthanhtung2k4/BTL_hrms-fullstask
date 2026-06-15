using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Application.Interfaces;

public interface IDepartmentService
{
    Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync();
    Task<Result<DepartmentDto>> GetByIdAsync(Guid id);
    Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentDto dto);
    Task<Result<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto);
    Task<Result> DeleteAsync(Guid id);
}
