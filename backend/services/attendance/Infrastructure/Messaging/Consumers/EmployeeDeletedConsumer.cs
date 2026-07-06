using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.Attendance.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.Attendance.Infrastructure.Messaging.Consumers;

public class EmployeeDeletedConsumer : IConsumer<IntegrationEvent<EmployeeDeletedPayload>>
{
    private readonly AttendanceDbContext _dbContext;

    public EmployeeDeletedConsumer(AttendanceDbContext dbContext)
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
