using Microsoft.Extensions.DependencyInjection;

namespace Hrms.HrCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services (e.g. IEmployeeService, IAuthService)
        
        return services;
    }
}
