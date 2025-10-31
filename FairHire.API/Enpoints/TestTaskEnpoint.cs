using FairHire.Application.Feature.TestTaskFeature.Commands;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;

namespace FairHire.API.Enpoints;

public static class TestTaskEnpoint
{
    public static void MapTestTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/test-task", async (CreateTestTaskCommand command, 
            CreateTestTaskRequest request, CancellationToken ct) => 
        {
            var task = await command.ExecuteAsync(request, ct);
            return Results.Ok(task);
        });
    }
}
