using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Application.Interfaces;

public interface IEmployeeService
{
    Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(ClaimsPrincipal currentUser);
    Task<Result<EmployeeDto>> GetByIdAsync(Guid id);
    Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto dto);
    Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeDto dto);
    Task<Result> ChangeStatusAsync(Guid id, ChangeStatusDto dto);
    Task<Result> DeleteAsync(Guid id);
}
