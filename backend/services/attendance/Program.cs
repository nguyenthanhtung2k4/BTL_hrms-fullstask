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
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmployeeProjections') AND name = 'IsDeleted') " +
        "BEGIN " +
        "    ALTER TABLE dbo.EmployeeProjections ADD IsDeleted BIT NOT NULL DEFAULT 0; " +
        "END"
    );
    await context.Database.ExecuteSqlRawAsync(
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmployeeProjections') AND name = 'HireDate') " +
        "ALTER TABLE dbo.EmployeeProjections ADD HireDate DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME();"
    );
    await context.Database.ExecuteSqlRawAsync(
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AttendanceRecords') AND name = 'CheckInReason') " +
        "BEGIN " +
        "    ALTER TABLE dbo.AttendanceRecords ADD CheckInReason NVARCHAR(MAX) NULL; " +
        "END"
    );
    await context.Database.ExecuteSqlRawAsync(
        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AttendanceRecords') AND name = 'CheckOutReason') " +
        "BEGIN " +
        "    ALTER TABLE dbo.AttendanceRecords ADD CheckOutReason NVARCHAR(MAX) NULL; " +
        "END"
    );
    await context.Database.ExecuteSqlRawAsync(
        "IF OBJECT_ID('dbo.AttendanceAdjustments', 'U') IS NULL " +
        "BEGIN " +
        "    CREATE TABLE dbo.AttendanceAdjustments ( " +
        "        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, " +
        "        EmployeeId UNIQUEIDENTIFIER NOT NULL, " +
        "        WorkDate DATE NOT NULL, " +
        "        ShiftId UNIQUEIDENTIFIER NOT NULL, " +
        "        ProposedCheckIn DATETIME2 NULL, " +
        "        ProposedCheckOut DATETIME2 NULL, " +
        "        Reason NVARCHAR(500) NOT NULL, " +
        "        Status NVARCHAR(30) NOT NULL, " +
        "        HandledByEmployeeId UNIQUEIDENTIFIER NULL, " +
        "        HandledAt DATETIME2 NULL, " +
        "        CreatedAt DATETIME2 NOT NULL, " +
        "        CreatedBy NVARCHAR(max) NULL, " +
        "        UpdatedAt DATETIME2 NULL, " +
        "        UpdatedBy NVARCHAR(max) NULL, " +
        "        CONSTRAINT FK_AttendanceAdjustments_EmployeeProjections_EmployeeId FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId), " +
        "        CONSTRAINT FK_AttendanceAdjustments_Shifts_ShiftId FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id), " +
        "        CONSTRAINT FK_AttendanceAdjustments_EmployeeProjections_HandledByEmployeeId FOREIGN KEY (HandledByEmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId) " +
        "    ); " +
        "END"
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

    // Seed or update LeaveTypes with proper Vietnamese diacritics
    var leaveTypes = await context.LeaveTypes.ToListAsync();
    var npn = leaveTypes.FirstOrDefault(t => t.Code == "NPN");
    if (npn != null) npn.Name = "Nghỉ phép năm";
    
    var no = leaveTypes.FirstOrDefault(t => t.Code == "NO");
    if (no != null) no.Name = "Nghỉ ốm";
    
    var nkl = leaveTypes.FirstOrDefault(t => t.Code == "NKL");
    if (nkl != null) nkl.Name = "Nghỉ không lương";
    
    var nts = leaveTypes.FirstOrDefault(t => t.Code == "NTS");
    if (nts != null) nts.Name = "Nghỉ thai sản";
    
    if (npn != null || no != null || nkl != null || nts != null)
    {
        await context.SaveChangesAsync();
    }
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
        EventNames.EmployeeStatusChanged,
        EventNames.EmployeeDeleted
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
