using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.PayrollReport.Infrastructure.Messaging.Consumers;

public class EmployeeStatusChangedConsumer : IConsumer<IntegrationEvent<EmployeeStatusChangedPayload>>
{
    private readonly PayrollReportDbContext _dbContext;

    public EmployeeStatusChangedConsumer(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<EmployeeStatusChangedPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var employee = await _dbContext.EmployeeProjections.FindAsync(payload.EmployeeId);
        if (employee != null)
        {
            employee.Status = payload.NewStatus;
            employee.LastSyncedAt = DateTime.UtcNow;
            _dbContext.EmployeeProjections.Update(employee);
            await _dbContext.SaveChangesAsync();
        }
    }
}
