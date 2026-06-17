using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Hrms.PayrollReport.Infrastructure.Persistence;
using Hrms.Shared.Persistence;
using Hrms.Shared.Messaging;
using Hrms.PayrollReport.Infrastructure.Messaging.Consumers;

namespace Hrms.PayrollReport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<PayrollReportDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(interceptor);
        });

        // Register MassTransit RabbitMQ and Consumers
        services.AddSharedMassTransit(configuration, x =>
        {
            x.AddConsumer<DepartmentEventConsumer>();
            x.AddConsumer<PositionEventConsumer>();
            x.AddConsumer<EmployeeEventConsumer>();
            x.AddConsumer<EmployeeStatusChangedConsumer>();
            x.AddConsumer<ContractEventConsumer>();
            x.AddConsumer<AttendanceEventConsumer>();
            x.AddConsumer<LeaveEventConsumer>();
        });
        
        return services;
    }
}
