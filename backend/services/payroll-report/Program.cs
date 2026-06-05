using Hrms.Contracts.Api;
using Hrms.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

var group = app.MapGroup("/api/payroll").WithTags("Payroll & Report");

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
