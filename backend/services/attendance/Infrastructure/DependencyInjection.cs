using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Hrms.Attendance.Infrastructure.Persistence;
using Hrms.Shared.Persistence;
using Hrms.Shared.Messaging;
using Hrms.Attendance.Infrastructure.Messaging.Consumers;

namespace Hrms.Attendance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register MassTransit RabbitMQ and Consumers
        services.AddSharedMassTransit(configuration, "attendance", x =>
        {
            x.AddConsumer<DepartmentEventConsumer>();
            x.AddConsumer<PositionEventConsumer>();
            x.AddConsumer<EmployeeEventConsumer>();
            x.AddConsumer<EmployeeStatusChangedConsumer>();
            x.AddConsumer<EmployeeDeletedConsumer>();
        });
        
        return services;
    }
}
