using FairHire.API.Helpers;
using FairHire.Application.Feature.SubmissionFeature.Command;
using FairHire.Application.Feature.SubmissionFeature.Models.Request;
using FairHire.Application.Feature.SubmissionFeature.Query;
using System.Security.Claims;

namespace FairHire.API.Enpoints;

public static class SubmissionEndpoints
{
    public static void MapSubmissionEndpoints(this IEndpointRouteBuilder app)
    {
        // CREATE submission (candidate)
        app.MapPost("/submissions", async (
            CreateSubmissionCommand command,
            SubmissionCreateRequest request,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            if (!AuthHelpers.TryGetUserId(user, out var _))
                return Results.Unauthorized();

            var result = await command.ExecuteAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("Candidate");

        // GET BY ID (company-own or candidate-own)
        app.MapGet("/submissions/{submissionId:guid}", async (
            Guid submissionId,
            GetSubmissionByIdQuery query,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
            var dto = await query.ExecuteAsync(submissionId, callerId, ct);
            return Results.Ok(dto);
        }).RequireAuthorization("CanOrCompany");

        // LIST BY SIMULATION (company)
        app.MapGet("/submissions/by-simulation/{simulationId:guid}", async (
            Guid simulationId,
            GetSubmissionsBySimulationQuery query,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
            var list = await query.ExecuteAsync(simulationId, callerId, ct);
            return Results.Ok(list);
        }).RequireAuthorization("Сompany");
    }
}

