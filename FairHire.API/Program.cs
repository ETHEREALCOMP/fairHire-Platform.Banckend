using FairHire.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// http or https://localhost:post/swagger/index.html for testing the API
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
