using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.Attendance.Domain.Entities;
using Hrms.Attendance.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.Attendance.Infrastructure.Messaging.Consumers;

public class PositionEventConsumer : IConsumer<IntegrationEvent<PositionPayload>>
{
    private readonly AttendanceDbContext _dbContext;

    public PositionEventConsumer(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<PositionPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var position = await _dbContext.PositionProjections.FindAsync(payload.PositionId);
        if (position == null)
        {
            position = new PositionProjection
            {
                Id = payload.PositionId,
                Code = payload.Code,
                Name = payload.Name,
                IsActive = payload.IsActive,
                LastSyncedAt = DateTime.UtcNow
            };
            _dbContext.PositionProjections.Add(position);
        }
        else
        {
            position.Code = payload.Code;
            position.Name = payload.Name;
            position.IsActive = payload.IsActive;
            position.LastSyncedAt = DateTime.UtcNow;
            _dbContext.PositionProjections.Update(position);
        }

        await _dbContext.SaveChangesAsync();
    }
}
