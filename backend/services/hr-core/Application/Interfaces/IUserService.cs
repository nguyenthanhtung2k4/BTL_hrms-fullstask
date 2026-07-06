using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Application.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<UserDto>>> GetAllAsync();
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result<UserDto>> GetByEmployeeIdAsync(Guid employeeId);
    Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto);
    Task<Result<UserDto>> UpdateRolesAsync(Guid id, UpdateUserRolesDto dto);
    Task<Result> ResetPasswordAsync(Guid id, ResetPasswordDto dto);
    Task<Result> ChangeStatusAsync(Guid id, bool isActive);
}
