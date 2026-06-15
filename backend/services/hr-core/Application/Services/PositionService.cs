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

public class PositionService : IPositionService
{
    private readonly HrDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public PositionService(HrDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<IEnumerable<PositionDto>>> GetAllAsync()
    {
        var positions = await _dbContext.Positions.ToListAsync();
        var dtos = positions.Select(p => MapToDto(p));
        return Result<IEnumerable<PositionDto>>.Success(dtos);
    }

    public async Task<Result<PositionDto>> GetByIdAsync(Guid id)
    {
        var position = await _dbContext.Positions.FindAsync(id);
        if (position == null)
        {
            return Result<PositionDto>.Failure("Position not found.");
        }

        return Result<PositionDto>.Success(MapToDto(position));
    }

    public async Task<Result<PositionDto>> CreateAsync(CreatePositionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<PositionDto>.Failure("Code and Name are required.");
        }

        if (await _dbContext.Positions.AnyAsync(p => p.Code == dto.Code))
        {
            return Result<PositionDto>.Failure($"Position with code '{dto.Code}' already exists.");
        }

        var position = new Position
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true
        };

        await _dbContext.Positions.AddAsync(position);
        await _dbContext.SaveChangesAsync();

        // Publish integration event
        var integrationEvent = new IntegrationEvent<PositionPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.PositionCreated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: new PositionPayload(position.Id, position.Code, position.Name, position.IsActive)
        );
        await _publishEndpoint.Publish(integrationEvent);

        return Result<PositionDto>.Success(MapToDto(position));
    }

    public async Task<Result<PositionDto>> UpdateAsync(Guid id, UpdatePositionDto dto)
    {
        var position = await _dbContext.Positions.FindAsync(id);
        if (position == null)
        {
            return Result<PositionDto>.Failure("Position not found.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<PositionDto>.Failure("Name is required.");
        }

        position.Name = dto.Name;
        position.Description = dto.Description;
        position.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();

        // Publish integration event
        var integrationEvent = new IntegrationEvent<PositionPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.PositionUpdated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: new PositionPayload(position.Id, position.Code, position.Name, position.IsActive)
        );
        await _publishEndpoint.Publish(integrationEvent);

        return Result<PositionDto>.Success(MapToDto(position));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var position = await _dbContext.Positions.FindAsync(id);
        if (position == null)
        {
            return Result.Failure("Position not found.");
        }

        if (await _dbContext.Employees.AnyAsync(e => e.PositionId == id))
        {
            return Result.Failure("Cannot delete position because it is assigned to employees.");
        }

        _dbContext.Positions.Remove(position);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Position deleted successfully.");
    }

    private static PositionDto MapToDto(Position p)
    {
        return new PositionDto(
            Id: p.Id,
            Code: p.Code,
            Name: p.Name,
            Description: p.Description,
            IsActive: p.IsActive,
            CreatedAt: p.CreatedAt,
            UpdatedAt: p.LastModifiedAt
        );
    }
}
