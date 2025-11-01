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
await app.SeedAsync();

ApplyMigrations(app);

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// http or https://localhost:post/swagger/index.html for testing the API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GuardMiddleware>();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapTestTaskEndpoints();

app.Run();


static void ApplyMigrations(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Check and apply pending migrations
        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            Console.WriteLine("Applying pending migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations found.");
        }
    }
}