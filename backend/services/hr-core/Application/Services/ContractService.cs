using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.HrCore.Domain.Entities;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hrms.HrCore.Application.Services;

public class ContractService : IContractService
{
    private readonly HrDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public ContractService(HrDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<IEnumerable<ContractDto>>> GetAllAsync()
    {
        var contracts = await _dbContext.Contracts
            .Include(c => c.Employee)
            .ToListAsync();

        var dtos = contracts.Select(c => MapToDto(c));
        return Result<IEnumerable<ContractDto>>.Success(dtos);
    }

    public async Task<Result<ContractDto>> GetByIdAsync(Guid id)
    {
        var contract = await _dbContext.Contracts
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
        {
            return Result<ContractDto>.Failure("Contract not found.");
        }

        return Result<ContractDto>.Success(MapToDto(contract));
    }

    public async Task<Result<ContractDto>> CreateAsync(CreateContractDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ContractNumber) || dto.EmployeeId == Guid.Empty || string.IsNullOrWhiteSpace(dto.ContractType))
        {
            return Result<ContractDto>.Failure("ContractNumber, EmployeeId, and ContractType are required.");
        }

        if (await _dbContext.Contracts.AnyAsync(c => c.ContractNo == dto.ContractNumber))
        {
            return Result<ContractDto>.Failure($"Contract number '{dto.ContractNumber}' already exists.");
        }

        if (!await _dbContext.Employees.AnyAsync(e => e.Id == dto.EmployeeId))
        {
            return Result<ContractDto>.Failure("Employee not found.");
        }

        var contract = new Contract
        {
            ContractNo = dto.ContractNumber,
            EmployeeId = dto.EmployeeId,
            ContractType = dto.ContractType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            BaseSalary = dto.BaseSalary,
            Status = "Active"
        };


        await _dbContext.Contracts.AddAsync(contract);
        await _dbContext.SaveChangesAsync();

        // Publish contract salary payload event to RabbitMQ
        var payload = new ContractSalaryPayload(
            ContractId: contract.Id,
            EmployeeId: contract.EmployeeId,
            BaseSalary: contract.BaseSalary,
            EffectiveFrom: DateOnly.FromDateTime(contract.StartDate),
            EffectiveTo: contract.EndDate.HasValue ? DateOnly.FromDateTime(contract.EndDate.Value) : null,
            Status: contract.Status
        );

        var integrationEvent = new IntegrationEvent<ContractSalaryPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.ContractCreated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: payload
        );

        await _publishEndpoint.Publish(integrationEvent);

        return await GetByIdAsync(contract.Id);
    }

    public async Task<Result<ContractDto>> UpdateAsync(Guid id, UpdateContractDto dto)
    {
        var contract = await _dbContext.Contracts.FindAsync(id);
        if (contract == null)
        {
            return Result<ContractDto>.Failure("Contract not found.");
        }

        if (string.IsNullOrWhiteSpace(dto.ContractType))
        {
            return Result<ContractDto>.Failure("ContractType is required.");
        }

        contract.ContractType = dto.ContractType;
        contract.StartDate = dto.StartDate;
        contract.EndDate = dto.EndDate;
        contract.BaseSalary = dto.BaseSalary;
        contract.Status = dto.Status;

        await _dbContext.SaveChangesAsync();

        // Publish contract salary payload event to RabbitMQ
        var payload = new ContractSalaryPayload(
            ContractId: contract.Id,
            EmployeeId: contract.EmployeeId,
            BaseSalary: contract.BaseSalary,
            EffectiveFrom: DateOnly.FromDateTime(contract.StartDate),
            EffectiveTo: contract.EndDate.HasValue ? DateOnly.FromDateTime(contract.EndDate.Value) : null,
            Status: contract.Status
        );

        var integrationEvent = new IntegrationEvent<ContractSalaryPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.ContractUpdated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: payload
        );

        await _publishEndpoint.Publish(integrationEvent);

        return await GetByIdAsync(id);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var contract = await _dbContext.Contracts.FindAsync(id);
        if (contract == null)
        {
            return Result.Failure("Contract not found.");
        }

        _dbContext.Contracts.Remove(contract);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Contract deleted successfully.");
    }

    private static ContractDto MapToDto(Contract c)
    {
        return new ContractDto(
            Id: c.Id,
            ContractNumber: c.ContractNo,
            EmployeeId: c.EmployeeId,
            EmployeeName: c.Employee?.FullName ?? "Unknown",
            ContractType: c.ContractType,
            StartDate: c.StartDate,
            EndDate: c.EndDate,
            BaseSalary: c.BaseSalary,
            Status: c.Status,
            CreatedAt: c.CreatedAt,
            UpdatedAt: c.UpdatedAt
        );
    }


}
