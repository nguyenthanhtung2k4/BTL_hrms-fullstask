using Hrms.Contracts.Api;
using Hrms.Contracts.Events;
using Hrms.HrCore.Application;
using Hrms.HrCore.Infrastructure;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HrDbContext>();
    await DbInitializer.SeedAsync(context);
}

app.UseMiddleware<ExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapHealthChecks("/health");


var group = app.MapGroup("/api/hr").WithTags("HR Core");

group.MapGet("/info", () => Results.Ok(new ServiceInfoResponse(
    ServiceName: "hr-core",
    Version: "v1",
    Database: "HRMS_HrCoreDb",
    OwnedModules:
    [
        "Auth",
        "Users",
        "Roles",
        "Employees",
        "Departments",
        "Positions",
        "Contracts"
    ],
    PublishedEvents:
    [
        EventNames.DepartmentCreated,
        EventNames.DepartmentUpdated,
        EventNames.PositionCreated,
        EventNames.PositionUpdated,
        EventNames.EmployeeCreated,
        EventNames.EmployeeUpdated,
        EventNames.EmployeeStatusChanged,
        EventNames.ContractCreated,
        EventNames.ContractUpdated,
        EventNames.ContractTerminated
    ],
    ConsumedEvents: [])));

group.MapGet("/modules", () => Results.Ok(new[]
{
    "Auth/User/Role",
    "Employee",
    "Department",
    "Position",
    "Contract",
    "AuditLog",
    "Outbox"
}));

app.Run();
