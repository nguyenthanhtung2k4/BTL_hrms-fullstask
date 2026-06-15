var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/", () => Results.Ok(new
{
    service = "hrms-api-gateway",
    routes = new[] { "/api/v1/hr/*", "/api/v1/attendance/*", "/api/v1/payroll/*" }
}));

app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();
