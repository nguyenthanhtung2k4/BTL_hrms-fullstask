using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.HrCore.Domain.Entities;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Hrms.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Hrms.HrCore.Application.Services;

public class UserService : IUserService
{
    private readonly HrDbContext _dbContext;

    public UserService(HrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync()
    {
        var users = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();

        var dtos = users.Select(u => MapToDto(u));
        return Result<IEnumerable<UserDto>>.Success(dtos);
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> GetByEmployeeIdAsync(Guid employeeId)
    {
        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found for this employee.");
        }

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        if (dto.EmployeeId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return Result<UserDto>.Failure("EmployeeId, Email, and Password are required.");
        }

        var employee = await _dbContext.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
        {
            return Result<UserDto>.Failure("Employee not found.");
        }

        if (await _dbContext.Users.AnyAsync(u => u.EmployeeId == dto.EmployeeId))
        {
            return Result<UserDto>.Failure("This employee already has a login account.");
        }

        if (await _dbContext.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return Result<UserDto>.Failure("A user account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            Email = dto.Email,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            IsActive = true
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        if (dto.Roles != null && dto.Roles.Any())
        {
            var dbRoles = await _dbContext.Roles
                .Where(r => dto.Roles.Contains(r.Name))
                .ToListAsync();

            foreach (var role in dbRoles)
            {
                await _dbContext.UserRoles.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        return await GetByIdAsync(user.Id);
    }

    public async Task<Result<UserDto>> UpdateRolesAsync(Guid id, UpdateUserRolesDto dto)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        // Remove old roles
        _dbContext.UserRoles.RemoveRange(user.UserRoles);
        await _dbContext.SaveChangesAsync();

        // Add new roles
        if (dto.Roles != null && dto.Roles.Any())
        {
            var dbRoles = await _dbContext.Roles
                .Where(r => dto.Roles.Contains(r.Name))
                .ToListAsync();

            foreach (var role in dbRoles)
            {
                await _dbContext.UserRoles.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        return await GetByIdAsync(id);
    }

    public async Task<Result> ResetPasswordAsync(Guid id, ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return Result.Failure("New password cannot be empty.");
        }

        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Password reset successfully.");
    }

    public async Task<Result> ChangeStatusAsync(Guid id, bool isActive)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        user.IsActive = isActive;
        await _dbContext.SaveChangesAsync();

        return Result.Success(isActive ? "Account activated successfully." : "Account deactivated successfully.");
    }

    private static UserDto MapToDto(User u)
    {
        return new UserDto(
            Id: u.Id,
            EmployeeId: u.EmployeeId,
            Email: u.Email,
            IsActive: u.IsActive,
            Roles: u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            LastLoginAt: u.LastLoginAt
        );
    }
}
