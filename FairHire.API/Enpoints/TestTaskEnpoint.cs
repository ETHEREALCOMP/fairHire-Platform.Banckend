using FairHire.Application.Feature.TestTaskFeature.Commands;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Application.Feature.TestTaskFeature.Queries;
using FairHire.Application.Feature.TestTaskFeature.Query;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FairHire.API.Enpoints;

public static class TestTaskEnpoint
{
    public static void MapTestTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/test-task", async (CreateTestTaskCommand command, CreateTestTaskRequest request, 
            ClaimsPrincipal user, AppDbContext context,
            UserManager<User> userManager, CancellationToken ct) => 
        {
            var callerIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(callerIdStr, out var callerId))
                return Results.Unauthorized();

            var companyProfile = await context.CompanyProfiles.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == callerId, ct);

            if (companyProfile is null)
                return Results.Forbid();

            var result = await command.ExecuteAsync(callerId, request, ct);

            return Results.Ok(result);

        }).RequireAuthorization("Company");

        app.MapPatch("/test-task/{taskId:guid}", async (Guid taskId, ClaimsPrincipal user, 
            UpdateTestTaskCommand command,AppDbContext context,
            UpdateTestTaskRequest request, CancellationToken ct) => 
        {
            var callerIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(callerIdStr, out var callerId))
                return Results.Unauthorized();

            var companyProfile = await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);

            if (!companyProfile)
                return Results.Forbid();

            var result = await command.ExecuteAsync(callerId, taskId, request, ct);
            return Results.Ok(result);

        }).RequireAuthorization("Company");

        app.MapGet("/test-task/all/{companyId:guid}", async (Guid companyId, GetAllTestTaskQuery query, 
            CancellationToken ct) =>
        {
            var result = await query.ExecuteAsync(companyId, ct);
            return Results.Ok(result);

        }).RequireAuthorization("devOrCompany");

        app.MapGet("/test-task/{taskId:guid}", async (Guid taskId,
            GetByIdTestTaskQuery query, CancellationToken ct) =>
        {
            var result = await query.ExecuteAsync(taskId, ct);
            return Results.Ok(result);

        }).RequireAuthorization("devOrCompany");
    }
}
