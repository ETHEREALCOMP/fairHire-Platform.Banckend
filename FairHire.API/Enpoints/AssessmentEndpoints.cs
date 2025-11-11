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
        app.MapPost("/assessments", async (
            CreateAssessmentCommand command,
            AssessmentCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            if (!AuthHelpers.TryGetUserId(user, out var callerId))
                return Results.Unauthorized();

            var isCompany = await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);
            if (!isCompany) return Results.Forbid();

            var result = await command.ExecuteAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("Company");

        // GET BY ID (company-own or candidate-own)
        app.MapGet("/assessments/{assessmentId:guid}", async (
            Guid assessmentId,
            GetAssessmentByIdQuery query,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
            var dto = await query.ExecuteAsync(assessmentId, callerId, ct);
            return Results.Ok(dto);
        }).RequireAuthorization("CanOrCompany");
    }
}
