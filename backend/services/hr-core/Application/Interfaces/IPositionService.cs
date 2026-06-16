using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Application.Interfaces;

public interface IPositionService
{
    Task<Result<IEnumerable<PositionDto>>> GetAllAsync();
    Task<Result<PositionDto>> GetByIdAsync(Guid id);
    Task<Result<PositionDto>> CreateAsync(CreatePositionDto dto);
    Task<Result<PositionDto>> UpdateAsync(Guid id, UpdatePositionDto dto);
    Task<Result> DeleteAsync(Guid id);
}
