using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hrms.Shared.Diagnostics;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddSharedHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        var dbConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(dbConnectionString))
        {
            healthChecks.AddTypeActivatedCheck<SqlServerHealthCheck>(
                "sqlserver", 
                args: new object[] { dbConnectionString });
        }

        var rabbitHost = configuration["RabbitMQ:Host"];
        if (!string.IsNullOrEmpty(rabbitHost))
        {
            healthChecks.AddTypeActivatedCheck<RabbitMqHealthCheck>(
                "rabbitmq", 
                args: new object[] { rabbitHost });
        }

        return services;
    }
}
