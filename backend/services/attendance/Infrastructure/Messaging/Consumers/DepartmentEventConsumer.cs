using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.Attendance.Domain.Entities;
using Hrms.Attendance.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.Attendance.Infrastructure.Messaging.Consumers;

public class DepartmentEventConsumer : IConsumer<IntegrationEvent<DepartmentPayload>>
{
    private readonly AttendanceDbContext _dbContext;

    public DepartmentEventConsumer(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<DepartmentPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var department = await _dbContext.DepartmentProjections.FindAsync(payload.DepartmentId);
        if (department == null)
        {
            department = new DepartmentProjection
            {
                Id = payload.DepartmentId,
                Code = payload.Code,
                Name = payload.Name,
                IsActive = payload.IsActive,
                LastSyncedAt = DateTime.UtcNow
            };
            _dbContext.DepartmentProjections.Add(department);
        }
        else
        {
            department.Code = payload.Code;
            department.Name = payload.Name;
            department.IsActive = payload.IsActive;
            department.LastSyncedAt = DateTime.UtcNow;
            _dbContext.DepartmentProjections.Update(department);
        }

        await _dbContext.SaveChangesAsync();
    }
}
