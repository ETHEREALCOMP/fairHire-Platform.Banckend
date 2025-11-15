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
        app.MapPost("/submissions", async
            (CreateSubmissionCommand command,
            SubmissionCreateRequest request,
            ClaimsPrincipal user,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SubmissionEndpoints.Create");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var _))
                    return Results.Unauthorized();

                var result = await command.ExecuteAsync(request, ct);
                logger.LogInformation("Submission created successfully with ID: {SubmissionsId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in Create submission endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Candidate");

        // GET BY ID (company-own or candidate-own)
        app.MapGet("/submissions/{submissionId:guid}", async
            (Guid submissionId,
            GetSubmissionByIdQuery query,
            ClaimsPrincipal user,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SubmissionEndpoints.GetById");
            try
            {
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
                var result = await query.ExecuteAsync(submissionId, callerId, ct);
                logger.LogInformation("Submission getted successfully with ID: {SubmissionsId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in Get submission by ID endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("CanOrCompany");

        // LIST BY SIMULATION (company)
        app.MapGet("/submissions/by-simulation/{simulationId:guid}", async 
            (Guid simulationId,
            GetSubmissionsBySimulationQuery query,
            ClaimsPrincipal user,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SubmissionEndpoints.Get");
            try
            {
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
                var result = await query.ExecuteAsync(simulationId, callerId, ct);
                logger.LogInformation("Submission getted successfully : {List}", result);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
            logger.LogError("Error in Get submissions by simulation endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");
    }
}

