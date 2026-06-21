using Hrms.Contracts.Api;
using Hrms.Contracts.Events;
using Hrms.Attendance.Application;
using Hrms.Attendance.Infrastructure;
using Hrms.Shared.Middleware;
using Hrms.Shared.Security;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Attendance API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Hrms.Attendance.Infrastructure.Persistence.AttendanceDbContext>();
    await context.Database.EnsureCreatedAsync();
    await context.Database.ExecuteSqlRawAsync(
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmployeeProjections') AND name = 'HireDate') " +
        "ALTER TABLE dbo.EmployeeProjections ADD HireDate DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME();"
    );
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

var group = app.MapGroup("/api/v1/attendance").WithTags("Attendance");

group.MapGet("/info", () => Results.Ok(new ServiceInfoResponse(
    ServiceName: "attendance",
    Version: "v1",
    Database: "HRMS_AttendanceDb",
    OwnedModules:
    [
        "EmployeeProjections",
        "Shifts",
        "WorkSchedules",
        "AttendanceRecords",
        "LeaveRequests",
        "Timesheets"
    ],
    PublishedEvents:
    [
        EventNames.AttendanceRecorded,
        EventNames.LeaveApproved
    ],
    ConsumedEvents:
    [
        EventNames.DepartmentCreated,
        EventNames.DepartmentUpdated,
        EventNames.PositionCreated,
        EventNames.PositionUpdated,
        EventNames.EmployeeCreated,
        EventNames.EmployeeUpdated,
        EventNames.EmployeeStatusChanged
    ])));

group.MapGet("/modules", () => Results.Ok(new[]
{
    "EmployeeProjection",
    "Shift",
    "WorkSchedule",
    "AttendanceRecord",
    "LeaveRequest",
    "Timesheet",
    "Inbox",
    "Outbox"
}));

app.Run();
