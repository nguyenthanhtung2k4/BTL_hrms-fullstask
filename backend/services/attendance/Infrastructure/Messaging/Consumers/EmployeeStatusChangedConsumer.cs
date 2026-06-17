using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.Attendance.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.Attendance.Infrastructure.Messaging.Consumers;

public class EmployeeStatusChangedConsumer : IConsumer<IntegrationEvent<EmployeeStatusChangedPayload>>
{
    private readonly AttendanceDbContext _dbContext;

    public EmployeeStatusChangedConsumer(AttendanceDbContext dbContext)
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
