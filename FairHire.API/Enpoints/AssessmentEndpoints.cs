using FairHire.API.Helpers;
using FairHire.Application.Feature.AssessmentFeature.Command;
using FairHire.Application.Feature.AssessmentFeature.Models.Request;
using FairHire.Application.Feature.AssessmentFeature.Query;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FairHire.API.Enpoints;

public static class AssessmentEndpoints
{
    public static void MapAssessmentEndpoints(this IEndpointRouteBuilder app)
    {
        // CREATE assessment (company)
        app.MapPost("/assessments", async 
            (CreateAssessmentCommand command,
            AssessmentCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("AssessmentEndpoints.Create");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                var result = await command.ExecuteAsync(request, ct);
                logger.LogInformation("Assessment created successfully with ID: {AssessmentId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error creating assessment: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // GET BY ID (company-own or candidate-own)
        app.MapGet("/assessments/{assessmentId:guid}", async
            (Guid assessmentId,
            GetAssessmentByIdQuery query,
            ClaimsPrincipal user,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("AssessmentEndpoints.GetById");
            try
            {
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
                var result = await query.ExecuteAsync(assessmentId, callerId, ct);
                logger.LogInformation("Assessment getted successfully with ID: {AssessmentId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error retrieving assessment by ID: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("CanOrCompany");
    }
}
