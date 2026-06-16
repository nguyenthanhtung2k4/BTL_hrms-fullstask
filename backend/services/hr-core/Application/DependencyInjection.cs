using Microsoft.Extensions.DependencyInjection;
using Hrms.HrCore.Application.Interfaces;
using Hrms.HrCore.Application.Services;

namespace Hrms.HrCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IContractService, ContractService>();
        
        return services;


    }
}

