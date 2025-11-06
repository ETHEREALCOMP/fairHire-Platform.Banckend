using FairHire.Application.Feature.TestTaskFeature.Commands;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Application.Feature.TestTaskFeature.Query;

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
        }).RequireAuthorization("Company");

        app.MapPatch("/test-task{taskId:guid}", async (Guid taskId, UpdateTestTaskCommand command, 
            UpdateTestTaskRequest request, CancellationToken ct) => 
        {
            var task = await command.ExecuteAsync(taskId, request, ct);
            return Results.Ok(task);
        }).RequireAuthorization("Company");

        app.MapGet("/test-task{taskId:guid}", async (Guid taskId,
            GetByIdTestTaskQuery query, CancellationToken ct) =>
        {
            var res = await query.ExecuteAsync(taskId, ct);
            return Results.Ok(res);
        }).RequireAuthorization("devOrCompany");
    }
}
