using System;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Domain.Entities;
using Hrms.PayrollReport.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hrms.PayrollReport.Infrastructure.Messaging.Consumers;

public class ContractEventConsumer : IConsumer<IntegrationEvent<ContractSalaryPayload>>
{
    private readonly PayrollReportDbContext _dbContext;

    public ContractEventConsumer(PayrollReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<IntegrationEvent<ContractSalaryPayload>> context)
    {
        var message = context.Message;
        var payload = message.Payload;

        var salaryProj = await _dbContext.EmployeeSalaryProjections
            .FirstOrDefaultAsync(s => s.ContractId == payload.ContractId);

        if (salaryProj == null)
        {
            salaryProj = new EmployeeSalaryProjection
            {
                Id = Guid.NewGuid(),
                EmployeeId = payload.EmployeeId,
                ContractId = payload.ContractId,
                BaseSalary = payload.BaseSalary,
                EffectiveFrom = payload.EffectiveFrom,
                EffectiveTo = payload.EffectiveTo,
                Status = payload.Status,
                LastSyncedAt = DateTime.UtcNow
            };
            _dbContext.EmployeeSalaryProjections.Add(salaryProj);
        }
        else
        {
            salaryProj.BaseSalary = payload.BaseSalary;
            salaryProj.EffectiveFrom = payload.EffectiveFrom;
            salaryProj.EffectiveTo = payload.EffectiveTo;
            salaryProj.Status = payload.Status;
            salaryProj.LastSyncedAt = DateTime.UtcNow;
            _dbContext.EmployeeSalaryProjections.Update(salaryProj);
        }

        await _dbContext.SaveChangesAsync();
    }
}
