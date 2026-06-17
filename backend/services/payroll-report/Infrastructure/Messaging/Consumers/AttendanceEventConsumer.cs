using System;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Domain.Entities;
using Hrms.PayrollReport.Infrastructure.Persistence;
using MassTransit;

namespace Hrms.PayrollReport.Infrastructure.Messaging.Consumers;

public class AttendanceEventConsumer : IConsumer<IntegrationEvent<AttendanceRecordedPayload>>
{
    private readonly PayrollReportDbContext _dbContext;

    public AttendanceEventConsumer(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<AttendanceRecordedPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var attendance = await _dbContext.AttendanceProjections.FindAsync(payload.AttendanceRecordId);
        if (attendance == null)
        {
            attendance = new AttendanceProjection
            {
                Id = payload.AttendanceRecordId,
                EmployeeId = payload.EmployeeId,
                WorkDate = payload.WorkDate,
                WorkedMinutes = payload.WorkedMinutes,
                Status = payload.Status,
                LastSyncedAt = DateTime.UtcNow
            };
            _dbContext.AttendanceProjections.Add(attendance);
        }
        else
        {
            attendance.WorkedMinutes = payload.WorkedMinutes;
            attendance.Status = payload.Status;
            attendance.LastSyncedAt = DateTime.UtcNow;
            _dbContext.AttendanceProjections.Update(attendance);
        }

        await _dbContext.SaveChangesAsync();
    }
}
