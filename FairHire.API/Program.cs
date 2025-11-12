using FairHire.API;
using FairHire.API.Enpoints;
using FairHire.API.Middleware;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var opentelemetryBuilder = builder.Services.AddOpenTelemetry();

opentelemetryBuilder.ConfigureResource(x => x.AddService(builder.Environment.ApplicationName));

opentelemetryBuilder.WithMetrics(
    metrics => metrics.AddAspNetCoreInstrumentation()
    .AddConsoleExporter()
    .AddPrometheusExporter());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FairHireDbContext>();
    dbContext.Database.Migrate();
    await app.SeedAsync();
}

app.UseCors();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GuardMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapAssessmentEndpoints();
app.MapSimulationEndpoints();
app.MapSubmissionEndpoints();
app.MapTaskTemplateEndpoints();
app.MapPrometheusScrapingEndpoint();

app.Run();
