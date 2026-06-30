using Hrms.Contracts.Api;
using Hrms.Contracts.Events;
using Hrms.PayrollReport.Application;
using Hrms.PayrollReport.Infrastructure;
using Hrms.Shared.Middleware;
using Hrms.Shared.Security;
using Hrms.Shared.Diagnostics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payroll & Report API", Version = "v1" });
    
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
    var context = scope.ServiceProvider.GetRequiredService<Hrms.PayrollReport.Infrastructure.Persistence.PayrollReportDbContext>();
    await Hrms.PayrollReport.Infrastructure.Persistence.DbInitializer.SeedAsync(context);
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payroll & Report API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

var group = app.MapGroup("/api/v1/payroll").WithTags("Payroll & Report");

group.MapGet("/info", () => Results.Ok(new ServiceInfoResponse(
    ServiceName: "payroll-report",
    Version: "v1",
    Database: "HRMS_PayrollReportDb",
    OwnedModules:
    [
        "EmployeeProjections",
        "AttendanceProjections",
        "PayrollPeriods",
        "PayrollRules",
        "Allowances",
        "Deductions",
        "Payslips",
        "Reports"
    ],
    PublishedEvents:
    [
        EventNames.PayrollClosed
    ],
    ConsumedEvents:
    [
        EventNames.DepartmentCreated,
        EventNames.DepartmentUpdated,
        EventNames.PositionCreated,
        EventNames.PositionUpdated,
        EventNames.EmployeeCreated,
        EventNames.EmployeeUpdated,
        EventNames.EmployeeStatusChanged,
        EventNames.EmployeeDeleted,
        EventNames.ContractCreated,
        EventNames.ContractUpdated,
        EventNames.ContractTerminated,
        EventNames.AttendanceRecorded,
        EventNames.LeaveApproved
    ])));

group.MapGet("/modules", () => Results.Ok(new[]
{
    "EmployeeProjection",
    "AttendanceProjection",
    "PayrollPeriod",
    "PayrollRule",
    "Allowance",
    "Deduction",
    "Payslip",
    "Report",
    "Inbox",
    "Outbox"
}));

app.Run();
