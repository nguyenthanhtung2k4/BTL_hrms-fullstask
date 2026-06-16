using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Persistence;
using Hrms.Shared.Messaging;

namespace Hrms.HrCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add HTTP context access (needed by the audit interceptor to fetch the current user's email)
        services.AddHttpContextAccessor();
        
        // Register the Audit Interceptor as Scoped
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();

        // Register the DbContext pointing to SQL Server
        services.AddDbContext<HrDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register MassTransit RabbitMQ
        services.AddSharedMassTransit(configuration);
        
        return services;
    }
}

