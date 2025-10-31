using FairHire.API;
using FairHire.API.Enpoints;
using FairHire.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
await app.SeedAsync();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// http or https://localhost:post/swagger/index.html for testing the API
app.UseSwagger();
app.UseSwaggerUI();

//Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GuardMiddleware>();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapTestTaskEndpoints();

app.Run();
