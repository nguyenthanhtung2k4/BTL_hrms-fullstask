using Hrms.Contracts.Api;
using Hrms.Contracts.Events;
using Hrms.HrCore.Application;
using Hrms.HrCore.Infrastructure;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Security;
using Hrms.Shared.Diagnostics;
using Hrms.Shared.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HR Core API", Version = "v1" });
    
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
builder.Services.AddHostedService<Hrms.HrCore.Application.Services.ContractExpiryBackgroundService>();

var app = builder.Build();

app.UseStaticFiles();

// Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HrDbContext>();
    await DbInitializer.SeedAsync(context);
}

app.UseMiddleware<ExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR Core API v1");
    });
}



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");


var group = app.MapGroup("/api/v1/hr").WithTags("HR Core");

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
