using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Domain.Entities;
using Hrms.PayrollReport.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.PayrollReport.Infrastructure.Messaging.Consumers;

public class EmployeeEventConsumer : IConsumer<IntegrationEvent<EmployeeProjectionPayload>>
{
    private readonly PayrollReportDbContext _dbContext;

    public EmployeeEventConsumer(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<EmployeeProjectionPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var employee = await _dbContext.EmployeeProjections.FindAsync(payload.EmployeeId);
        if (employee == null)
        {
            employee = new EmployeeProjection
            {
                Id = payload.EmployeeId,
                EmployeeCode = payload.EmployeeCode,
                FullName = payload.FullName,
                Email = payload.Email,
                DepartmentId = payload.DepartmentId,
                PositionId = payload.PositionId,
                ManagerEmployeeId = payload.ManagerEmployeeId,
                Status = payload.Status,
                LastSyncedAt = DateTime.UtcNow
            };
            _dbContext.EmployeeProjections.Add(employee);
        }
        else
        {
            employee.EmployeeCode = payload.EmployeeCode;
            employee.FullName = payload.FullName;
            employee.Email = payload.Email;
            employee.DepartmentId = payload.DepartmentId;
            employee.PositionId = payload.PositionId;
            employee.ManagerEmployeeId = payload.ManagerEmployeeId;
            employee.Status = payload.Status;
            employee.LastSyncedAt = DateTime.UtcNow;
            _dbContext.EmployeeProjections.Update(employee);
        }

        await _dbContext.SaveChangesAsync();
    }
}
