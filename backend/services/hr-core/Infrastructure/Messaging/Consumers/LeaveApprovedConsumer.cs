using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.HrCore.Domain.Entities;
using Hrms.HrCore.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.HrCore.Infrastructure.Messaging.Consumers;

public class LeaveApprovedConsumer : IConsumer<IntegrationEvent<LeaveApprovedPayload>>
{
    private readonly HrDbContext _dbContext;

    public LeaveApprovedConsumer(HrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<LeaveApprovedPayload>> context)
    {
        var payload = context.Message.Payload;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EmployeeId = payload.EmployeeId,
            Title = "Đơn nghỉ phép đã được duyệt",
            Content = $"Đơn xin nghỉ phép của bạn từ ngày {payload.FromDate:dd/MM/yyyy} đến ngày {payload.ToDate:dd/MM/yyyy} ({payload.TotalDays} ngày) đã được phê duyệt.",
            Type = "LeaveApproval",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }
}
