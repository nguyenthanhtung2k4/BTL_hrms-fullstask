using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Domain.Entities;
using Hrms.PayrollReport.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.PayrollReport.Infrastructure.Messaging.Consumers;

public class LeaveEventConsumer : IConsumer<IntegrationEvent<LeaveApprovedPayload>>
{
    private readonly PayrollReportDbContext _dbContext;

    public LeaveEventConsumer(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<LeaveApprovedPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var leave = await _dbContext.LeaveProjections.FindAsync(payload.LeaveRequestId);
        if (leave == null)
        {
            leave = new LeaveProjection
            {
                Id = payload.LeaveRequestId,
                EmployeeId = payload.EmployeeId,
                FromDate = payload.FromDate,
                ToDate = payload.ToDate,
                TotalDays = payload.TotalDays,
                IsPaid = payload.Paid,
                LastSyncedAt = DateTime.UtcNow
            };
            _dbContext.LeaveProjections.Add(leave);
        }
        else
        {
            leave.FromDate = payload.FromDate;
            leave.ToDate = payload.ToDate;
            leave.TotalDays = payload.TotalDays;
            leave.IsPaid = payload.Paid;
            leave.LastSyncedAt = DateTime.UtcNow;
            _dbContext.LeaveProjections.Update(leave);
        }

        await _dbContext.SaveChangesAsync();
    }
}
