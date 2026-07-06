using System;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.HrCore.Domain.Entities;
using Hrms.HrCore.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hrms.HrCore.Infrastructure.Messaging.Consumers;

public class PayrollClosedConsumer : IConsumer<IntegrationEvent<PayrollClosedPayload>>
{
    private readonly HrDbContext _dbContext;

    public PayrollClosedConsumer(HrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<PayrollClosedPayload>> context)
    {
        var payload = context.Message.Payload;

        // Notify all active employees
        var activeEmployees = await _dbContext.Employees
            .Where(e => e.Status == "Active")
            .ToListAsync();

        foreach (var employee in activeEmployees)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                Title = "Phiếu lương mới",
                Content = $"Phiếu lương của bạn cho kỳ '{payload.PeriodName}' đã được phát hành. Vui lòng kiểm tra trong mục 'Phiếu lương của tôi'.",
                Type = "NewPayslip",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Notifications.Add(notification);
        }

        await _dbContext.SaveChangesAsync();
    }
}
