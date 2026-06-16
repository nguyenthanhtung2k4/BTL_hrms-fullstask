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

public class EmployeeService : IEmployeeService
{
    private readonly HrDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public EmployeeService(HrDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync()
    {
        var employees = await _dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Manager)
            .ToListAsync();

        var dtos = employees.Select(e => MapToDto(e));
        return Result<IEnumerable<EmployeeDto>>.Success(dtos);
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(Guid id)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return Result<EmployeeDto>.Failure("Employee not found.");
        }

        return Result<EmployeeDto>.Success(MapToDto(employee));
    }

    public async Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EmployeeCode) || string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
        {
            return Result<EmployeeDto>.Failure("EmployeeCode, FullName, and Email are required.");
        }

        if (await _dbContext.Employees.AnyAsync(e => e.EmployeeCode == dto.EmployeeCode))
        {
            return Result<EmployeeDto>.Failure($"Employee with code '{dto.EmployeeCode}' already exists.");
        }

        if (await _dbContext.Employees.AnyAsync(e => e.Email == dto.Email))
        {
            return Result<EmployeeDto>.Failure($"Employee with email '{dto.Email}' already exists.");
        }

        var department = await _dbContext.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
        {
            return Result<EmployeeDto>.Failure("Department not found.");
        }

        var position = await _dbContext.Positions.FindAsync(dto.PositionId);
        if (position == null)
        {
            return Result<EmployeeDto>.Failure("Position not found.");
        }

        if (dto.ManagerEmployeeId.HasValue && !await _dbContext.Employees.AnyAsync(e => e.Id == dto.ManagerEmployeeId))
        {
            return Result<EmployeeDto>.Failure("Manager employee not found.");
        }

        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            HireDate = dto.HireDate,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            ManagerEmployeeId = dto.ManagerEmployeeId,
            Status = "Active"
        };

        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync();

        // Publish event to RabbitMQ for Projection
        var payload = new EmployeeProjectionPayload(
            EmployeeId: employee.Id,
            EmployeeCode: employee.EmployeeCode,
            FullName: employee.FullName,
            Email: employee.Email,
            DepartmentId: employee.DepartmentId,
            DepartmentName: department.Name,
            PositionId: employee.PositionId,
            PositionName: position.Name,
            ManagerEmployeeId: employee.ManagerEmployeeId,
            Status: employee.Status
        );

        var integrationEvent = new IntegrationEvent<EmployeeProjectionPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.EmployeeCreated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: payload
        );

        await _publishEndpoint.Publish(integrationEvent);

        return await GetByIdAsync(employee.Id);
    }

    public async Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeDto dto)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            return Result<EmployeeDto>.Failure("Employee not found.");
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return Result<EmployeeDto>.Failure("FullName is required.");
        }

        var department = await _dbContext.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
        {
            return Result<EmployeeDto>.Failure("Department not found.");
        }

        var position = await _dbContext.Positions.FindAsync(dto.PositionId);
        if (position == null)
        {
            return Result<EmployeeDto>.Failure("Position not found.");
        }

        if (dto.ManagerEmployeeId.HasValue)
        {
            if (dto.ManagerEmployeeId == id)
            {
                return Result<EmployeeDto>.Failure("An employee cannot be their own manager.");
            }
            if (!await _dbContext.Employees.AnyAsync(e => e.Id == dto.ManagerEmployeeId))
            {
                return Result<EmployeeDto>.Failure("Manager employee not found.");
            }
        }

        employee.FullName = dto.FullName;
        employee.Phone = dto.Phone;
        employee.Gender = dto.Gender;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.HireDate = dto.HireDate;
        employee.DepartmentId = dto.DepartmentId;
        employee.PositionId = dto.PositionId;
        employee.ManagerEmployeeId = dto.ManagerEmployeeId;
        employee.Status = dto.Status;

        await _dbContext.SaveChangesAsync();

        // Publish event to RabbitMQ for Projection
        var payload = new EmployeeProjectionPayload(
            EmployeeId: employee.Id,
            EmployeeCode: employee.EmployeeCode,
            FullName: employee.FullName,
            Email: employee.Email,
            DepartmentId: employee.DepartmentId,
            DepartmentName: department.Name,
            PositionId: employee.PositionId,
            PositionName: position.Name,
            ManagerEmployeeId: employee.ManagerEmployeeId,
            Status: employee.Status
        );

        var integrationEvent = new IntegrationEvent<EmployeeProjectionPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.EmployeeUpdated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: payload
        );

        await _publishEndpoint.Publish(integrationEvent);

        return await GetByIdAsync(id);
    }

    public async Task<Result> ChangeStatusAsync(Guid id, ChangeStatusDto dto)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            return Result.Failure("Employee not found.");
        }

        var oldStatus = employee.Status;
        employee.Status = dto.NewStatus;

        // Add to history
        var history = new EmployeeStatusHistory
        {
            Id = Guid.NewGuid(),
            EmployeeId = id,
            OldStatus = oldStatus,
            NewStatus = dto.NewStatus,
            Reason = dto.Reason,
            ChangedByUserId = dto.ChangedByUserId,
            ChangedAt = DateTime.UtcNow
        };
        await _dbContext.EmployeeStatusHistories.AddAsync(history);
        await _dbContext.SaveChangesAsync();

        // Publish Status Changed event
        var payload = new EmployeeStatusChangedPayload(
            EmployeeId: id,
            OldStatus: oldStatus,
            NewStatus: dto.NewStatus,
            Reason: dto.Reason
        );

        var integrationEvent = new IntegrationEvent<EmployeeStatusChangedPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.EmployeeStatusChanged,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: payload
        );

        await _publishEndpoint.Publish(integrationEvent);

        return Result.Success($"Status changed from {oldStatus} to {dto.NewStatus} successfully.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            return Result.Failure("Employee not found.");
        }

        // Check if employee has contracts
        if (await _dbContext.Contracts.AnyAsync(c => c.EmployeeId == id))
        {
            return Result.Failure("Cannot delete employee because they have contract records.");
        }

        // Check if employee has user account
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
        if (user != null)
        {
            _dbContext.Users.Remove(user);
        }

        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Employee deleted successfully.");
    }

    private static EmployeeDto MapToDto(Employee e)
    {
        return new EmployeeDto(
            Id: e.Id,
            EmployeeCode: e.EmployeeCode,
            FullName: e.FullName,
            Email: e.Email,
            Phone: e.Phone,
            Gender: e.Gender,
            DateOfBirth: e.DateOfBirth,
            HireDate: e.HireDate,
            DepartmentId: e.DepartmentId,
            DepartmentName: e.Department?.Name ?? "Unknown",
            PositionId: e.PositionId,
            PositionName: e.Position?.Name ?? "Unknown",
            ManagerEmployeeId: e.ManagerEmployeeId,
            ManagerName: e.Manager?.FullName,
            Status: e.Status,
            CreatedAt: e.CreatedAt,
            UpdatedAt: e.UpdatedAt
        );
    }

}
