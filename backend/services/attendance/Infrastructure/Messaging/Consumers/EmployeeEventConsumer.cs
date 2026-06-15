using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.Attendance.Domain.Entities;
using Hrms.Attendance.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.Attendance.Infrastructure.Messaging.Consumers;

public class EmployeeEventConsumer : IConsumer<IntegrationEvent<EmployeeProjectionPayload>>
{
    private readonly AttendanceDbContext _dbContext;

    public EmployeeEventConsumer(AttendanceDbContext dbContext)
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
