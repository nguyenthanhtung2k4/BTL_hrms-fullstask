using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hrms.HrCore.Domain.Entities;
using Hrms.HrCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hrms.HrCore.Application.Services;

public class ContractExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContractExpiryBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(12);

    public ContractExpiryBackgroundService(IServiceProvider serviceProvider, ILogger<ContractExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Contract Expiry Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiringContractsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking expiring contracts.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckExpiringContractsAsync()
    {
        _logger.LogInformation("Scanning for contracts expiring in less than 30 days...");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrDbContext>();

        var today = DateTime.Today;
        var thresholdDate = today.AddDays(30);

        // Find active contracts expiring within 30 days
        var expiringContracts = await dbContext.Contracts
            .Include(c => c.Employee)
            .Where(c => c.Status == "Active" && c.EndDate.HasValue && c.EndDate.Value >= today && c.EndDate.Value <= thresholdDate)
            .ToListAsync();

        if (!expiringContracts.Any())
        {
            _logger.LogInformation("No expiring contracts found.");
            return;
        }

        _logger.LogInformation("Found {Count} expiring contracts. Processing notifications...", expiringContracts.Count);

        // Get HR & Admin EmployeeIds to notify them as well
        var hrAndAdminUserEmployeeIds = await dbContext.UserRoles
            .Include(ur => ur.Role)
            .Include(ur => ur.User)
            .Where(ur => ur.Role.Name == "Admin" || ur.Role.Name == "HR")
            .Select(ur => ur.User.EmployeeId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToListAsync();

        foreach (var contract in expiringContracts)
        {
            // 1. Notify the Employee themselves
            var employeeContent = $"Hợp đồng số {contract.ContractNo} của bạn sẽ hết hạn vào ngày {contract.EndDate.Value:dd/MM/yyyy}. Vui lòng liên hệ bộ phận nhân sự để thực hiện gia hạn.";
            var employeeNotificationExists = await dbContext.Notifications
                .AnyAsync(n => n.EmployeeId == contract.EmployeeId && n.Type == "ContractExpiry" && n.Content.Contains(contract.ContractNo));

            if (!employeeNotificationExists)
            {
                dbContext.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = contract.EmployeeId,
                    Title = "Cảnh báo hết hạn hợp đồng",
                    Content = employeeContent,
                    Type = "ContractExpiry",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 2. Notify HR & Admin team
            var hrContent = $"Hợp đồng số {contract.ContractNo} của nhân viên {contract.Employee?.FullName} (Mã: {contract.Employee?.EmployeeCode}) sẽ hết hạn vào ngày {contract.EndDate.Value:dd/MM/yyyy}.";
            foreach (var hrEmpId in hrAndAdminUserEmployeeIds)
            {
                // Avoid duplicating employee notification if they are also HR/Admin
                if (hrEmpId == contract.EmployeeId) continue;

                var hrNotificationExists = await dbContext.Notifications
                    .AnyAsync(n => n.EmployeeId == hrEmpId && n.Type == "ContractExpiry" && n.Content.Contains(contract.ContractNo));

                if (!hrNotificationExists)
                {
                    dbContext.Notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = hrEmpId,
                        Title = "Cảnh báo hết hạn hợp đồng nhân viên",
                        Content = hrContent,
                        Type = "ContractExpiry",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Notifications for expiring contracts processed successfully.");
    }
}
