using System;
using System.Linq;
using System.Threading.Tasks;
using Hrms.HrCore.Domain.Entities;
using Hrms.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Hrms.HrCore.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(HrDbContext context)
    {
        // Ensure database is created or migrated
        await context.Database.EnsureCreatedAsync();

        // Seed Admin role if it does not exist
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Description = "Toan quyen he thong",
                CreatedAt = DateTime.UtcNow
            };
            await context.Roles.AddAsync(adminRole);
            await context.SaveChangesAsync();
        }

        // Seed other standard roles
        var standardRoles = new[] { "HR", "Manager", "Employee", "PayrollStaff" };
        foreach (var roleName in standardRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == roleName))
            {
                await context.Roles.AddAsync(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    Description = $"{roleName} role",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();

        // Seed default Admin User if none exists in database
        if (!await context.Users.AnyAsync())
        {
            // Seed a default Department first to satisfy employee constraints
            var defaultDept = await context.Departments.FirstOrDefaultAsync();
            if (defaultDept == null)
            {
                defaultDept = new Department
                {
                    Id = Guid.NewGuid(),
                    Code = "DEPT001",
                    Name = "Administration",
                    IsActive = true
                };
                await context.Departments.AddAsync(defaultDept);
                await context.SaveChangesAsync();
            }

            // Seed a default Position first to satisfy employee constraints
            var defaultPos = await context.Positions.FirstOrDefaultAsync();
            if (defaultPos == null)
            {
                defaultPos = new Position
                {
                    Id = Guid.NewGuid(),
                    Code = "POS001",
                    Name = "System Administrator",
                    IsActive = true
                };
                await context.Positions.AddAsync(defaultPos);
                await context.SaveChangesAsync();
            }

            // Create Administrator Employee profile
            var adminEmployee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = "EMP000",
                FullName = "System Administrator",
                Email = "admin@hrms.com",
                HireDate = DateTime.UtcNow,
                DepartmentId = defaultDept.Id,
                PositionId = defaultPos.Id,
                Status = "Active"
            };

            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync();

            // Create Administrator User Account
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                EmployeeId = adminEmployee.Id,
                Email = "admin@hrms.com",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                IsActive = true
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            // Map User to Admin Role
            var userRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();
        }
    }
}
