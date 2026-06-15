using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hrms.Shared.Messaging;

namespace Hrms.HrCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext, MassTransit/RabbitMQ
        // e.g. services.AddSharedMassTransit(configuration);
        
        return services;
    }
}
