using FairHire.API.Contract;
using System.Text.Json;

namespace FairHire.API.Problems;

public class Problem(IHostEnvironment env)
{
    public async Task WriteProblem(HttpContext ctx, int status, string title, Exception ex)
    {
        if (ctx.Response.HasStarted)
            return;

        ctx.Response.Clear();

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";

        var detail = env.IsDevelopment()
            ? ex.Message : "An unexpected error occurred."; // у проді без деталей; у Dev віддати ex.Message

        var payload = new
        {
            type = $"https://httpstatuses.io/{status}",
            title,
            status,
            detail,
            instance = ctx.Request.Path.Value,
            traceId = ctx.TraceIdentifier
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    public async Task WriteProblem(HttpContext ctx, int status, string title, string? detail = null)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";

        var payload = new ProblemContract
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = ctx.Request.Path,
            TraceId = ctx.TraceIdentifier,
            RequestId = ctx.Response.Headers["X-Request-Id"].ToString()
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
