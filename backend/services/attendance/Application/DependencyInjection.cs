using Microsoft.Extensions.DependencyInjection;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Attendance.Application.Services;

namespace Hrms.Attendance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IWorkScheduleService, WorkScheduleService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<ITimesheetService, TimesheetService>();
        
        return services;
    }
}

