using FairHire.API;
using FairHire.API.Enpoints;
using FairHire.API.Middleware;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    await app.SeedAsync();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GuardMiddleware>();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapTestTaskEndpoints();

app.Run();
