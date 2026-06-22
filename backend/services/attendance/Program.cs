using Hrms.Contracts.Api;
using Hrms.Contracts.Events;
using Hrms.Attendance.Application;
using Hrms.Attendance.Infrastructure;
using Hrms.Shared.Middleware;
using Hrms.Shared.Security;
using Hrms.Shared.Diagnostics;
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

builder.Services.AddSharedHealthChecks(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Hrms.Attendance.Infrastructure.Persistence.AttendanceDbContext>();
    await context.Database.EnsureCreatedAsync();
    await context.Database.ExecuteSqlRawAsync(
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmployeeProjections') AND name = 'HireDate') " +
        "ALTER TABLE dbo.EmployeeProjections ADD HireDate DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME();"
    );
    await context.Database.ExecuteSqlRawAsync(
        "IF OBJECT_ID('dbo.LeaveBalances', 'U') IS NULL " +
        "BEGIN " +
        "    CREATE TABLE dbo.LeaveBalances ( " +
        "        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, " +
        "        EmployeeId UNIQUEIDENTIFIER NOT NULL, " +
        "        LeaveTypeId UNIQUEIDENTIFIER NOT NULL, " +
        "        Year INT NOT NULL, " +
        "        EntitledDays DECIMAL(18,2) NOT NULL, " +
        "        UsedDays DECIMAL(18,2) NOT NULL, " +
        "        CreatedAt DATETIME2 NOT NULL, " +
        "        CreatedBy NVARCHAR(max) NULL, " +
        "        UpdatedAt DATETIME2 NULL, " +
        "        UpdatedBy NVARCHAR(max) NULL, " +
        "        CONSTRAINT FK_LeaveBalances_EmployeeProjections_EmployeeId FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId), " +
        "        CONSTRAINT FK_LeaveBalances_LeaveTypes_LeaveTypeId FOREIGN KEY (LeaveTypeId) REFERENCES dbo.LeaveTypes(Id) " +
        "    ); " +
        "    CREATE UNIQUE INDEX IX_LeaveBalances_EmployeeId_LeaveTypeId_Year ON dbo.LeaveBalances(EmployeeId, LeaveTypeId, Year); " +
        "END"
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
