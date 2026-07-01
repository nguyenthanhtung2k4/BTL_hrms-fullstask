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
using Microsoft.Extensions.Caching.Memory;

namespace Hrms.HrCore.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly HrDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "departments_all";

    public DepartmentService(HrDbContext dbContext, IPublishEndpoint publishEndpoint, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public async Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync()
    {
        if (!_cache.TryGetValue(CacheKey, out IEnumerable<DepartmentDto>? dtos))
        {
            var departments = await _dbContext.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.ManagerEmployee)
                .ToListAsync();

            dtos = departments.Select(d => MapToDto(d)).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(CacheKey, dtos, cacheOptions);
        }

        return Result<IEnumerable<DepartmentDto>>.Success(dtos!);
    }

    public async Task<Result<DepartmentDto>> GetByIdAsync(Guid id)
    {
        var department = await _dbContext.Departments
            .Include(d => d.ParentDepartment)
            .Include(d => d.ManagerEmployee)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return Result<DepartmentDto>.Failure("Department not found.");
        }

        return Result<DepartmentDto>.Success(MapToDto(department));
    }

    public async Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<DepartmentDto>.Failure("Code and Name are required.");
        }

        if (await _dbContext.Departments.AnyAsync(d => d.Code == dto.Code))
        {
            return Result<DepartmentDto>.Failure($"Department with code '{dto.Code}' already exists.");
        }

        if (dto.ParentDepartmentId.HasValue && !await _dbContext.Departments.AnyAsync(d => d.Id == dto.ParentDepartmentId))
        {
            return Result<DepartmentDto>.Failure("Parent department not found.");
        }

        if (dto.ManagerEmployeeId.HasValue && !await _dbContext.Employees.AnyAsync(e => e.Id == dto.ManagerEmployeeId))
        {
            return Result<DepartmentDto>.Failure("Manager employee not found.");
        }

        var department = new Department
        {
            Code = dto.Code,
            Name = dto.Name,
            ParentDepartmentId = dto.ParentDepartmentId,
            ManagerEmployeeId = dto.ManagerEmployeeId,
            IsActive = true
        };

        await _dbContext.Departments.AddAsync(department);
        await _dbContext.SaveChangesAsync();

        _cache.Remove(CacheKey);

        // Publish integration event
        var integrationEvent = new IntegrationEvent<DepartmentPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.DepartmentCreated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: new DepartmentPayload(department.Id, department.Code, department.Name, department.IsActive)
        );
        await _publishEndpoint.Publish(integrationEvent);

        // Fetch again to load navigations
        return await GetByIdAsync(department.Id);
    }

    public async Task<Result<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto)
    {
        var department = await _dbContext.Departments.FindAsync(id);
        if (department == null)
        {
            return Result<DepartmentDto>.Failure("Department not found.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<DepartmentDto>.Failure("Name is required.");
        }

        if (dto.ParentDepartmentId.HasValue)
        {
            if (dto.ParentDepartmentId == id)
            {
                return Result<DepartmentDto>.Failure("A department cannot be its own parent.");
            }
            if (!await _dbContext.Departments.AnyAsync(d => d.Id == dto.ParentDepartmentId))
            {
                return Result<DepartmentDto>.Failure("Parent department not found.");
            }
        }

        if (dto.ManagerEmployeeId.HasValue && !await _dbContext.Employees.AnyAsync(e => e.Id == dto.ManagerEmployeeId))
        {
            return Result<DepartmentDto>.Failure("Manager employee not found.");
        }

        department.Name = dto.Name;
        department.ParentDepartmentId = dto.ParentDepartmentId;
        department.ManagerEmployeeId = dto.ManagerEmployeeId;
        department.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();

        _cache.Remove(CacheKey);

        // Publish integration event
        var integrationEvent = new IntegrationEvent<DepartmentPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.DepartmentUpdated,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "hr-core",
            CorrelationId: null,
            Payload: new DepartmentPayload(department.Id, department.Code, department.Name, department.IsActive)
        );
        await _publishEndpoint.Publish(integrationEvent);

        return await GetByIdAsync(id);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var department = await _dbContext.Departments.FindAsync(id);
        if (department == null)
        {
            return Result.Failure("Department not found.");
        }

        if (await _dbContext.Departments.AnyAsync(d => d.ParentDepartmentId == id))
        {
            return Result.Failure("Cannot delete department because it has sub-departments associated with it.");
        }

        if (await _dbContext.Employees.AnyAsync(e => e.DepartmentId == id))
        {
            return Result.Failure("Cannot delete department because it has employees assigned to it.");
        }

        _dbContext.Departments.Remove(department);
        await _dbContext.SaveChangesAsync();

        _cache.Remove(CacheKey);

        return Result.Success("Department deleted successfully.");
    }

    public async Task<Result<IEnumerable<DepartmentDto>>> GetMyDepartmentsAsync(Guid managerId)
    {
        // Lấy department mà Manager này thuộc về
        var employee = await _dbContext.Employees.FindAsync(managerId);
        if (employee == null)
            return Result<IEnumerable<DepartmentDto>>.Failure("Employee not found.");

        var departments = await _dbContext.Departments
            .Include(d => d.ParentDepartment)
            .Include(d => d.ManagerEmployee)
            .Where(d => d.Id == employee.DepartmentId && d.IsActive)
            .ToListAsync();

        return Result<IEnumerable<DepartmentDto>>.Success(departments.Select(MapToDto));
    }

    private static DepartmentDto MapToDto(Department d)
    {
        return new DepartmentDto(
            Id: d.Id,
            Code: d.Code,
            Name: d.Name,
            ParentDepartmentId: d.ParentDepartmentId,
            ParentDepartmentName: d.ParentDepartment?.Name,
            ManagerEmployeeId: d.ManagerEmployeeId,
            ManagerName: d.ManagerEmployee?.FullName,
            IsActive: d.IsActive,
            CreatedAt: d.CreatedAt,
            UpdatedAt: d.UpdatedAt
        );
    }

}
