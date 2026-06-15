using Hrms.Contracts.Api;
using Hrms.Contracts.Events;
using Hrms.Attendance.Application;
using Hrms.Attendance.Infrastructure;
using Hrms.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

var group = app.MapGroup("/api/attendance").WithTags("Attendance");

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
