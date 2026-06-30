using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.PayrollReport.Infrastructure.Messaging.Consumers;

public class EmployeeDeletedConsumer : IConsumer<IntegrationEvent<EmployeeDeletedPayload>>
{
    private readonly PayrollReportDbContext _dbContext;

    public EmployeeDeletedConsumer(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<EmployeeDeletedPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var employee = await _dbContext.EmployeeProjections.FindAsync(payload.EmployeeId);
        if (employee != null)
        {
            employee.IsDeleted = true;
            employee.Status = "Resigned";
            employee.LastSyncedAt = DateTime.UtcNow;
            _dbContext.EmployeeProjections.Update(employee);
            await _dbContext.SaveChangesAsync();
        }
    }
}
