using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Application.Interfaces;

public interface IContractService
{
    Task<Result<IEnumerable<ContractDto>>> GetAllAsync();
    Task<Result<ContractDto>> GetByIdAsync(Guid id);
    Task<Result<ContractDto>> CreateAsync(CreateContractDto dto);
    Task<Result<ContractDto>> UpdateAsync(Guid id, UpdateContractDto dto);
    Task<Result> DeleteAsync(Guid id);
}
