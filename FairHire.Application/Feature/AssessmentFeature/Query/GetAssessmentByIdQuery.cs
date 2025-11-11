using FairHire.Application.Feature.AssessmentFeature.Models.Response;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.AssessmentFeature.Query;

public sealed class GetAssessmentByIdQuery(FairHireDbContext db)
{
    /// <summary>
    /// Дозвіл: company = власник симуляції, candidate = власник submission.
    /// </summary>
    public async Task<AssessmentResponse> ExecuteAsync(Guid assessmentId, Guid callerId,
        CancellationToken ct)
    {
        var a = await db.Assessments
            .AsNoTracking()
            .Include(x => x.Submission)
            .ThenInclude(s => s.Simulation)
            .FirstOrDefaultAsync(x => x.Id == assessmentId, ct)
            ?? throw new KeyNotFoundException("Assessment not found.");

        var isCompanyOwner = a.Submission.Simulation.CompanyId == callerId;
        var isCandidateOwner = a.Submission.CandidateUserId == callerId;

        if (!isCompanyOwner && !isCandidateOwner)
            throw new UnauthorizedAccessException("Access denied.");

        var scores = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(a.ScoresJson)
                     ?? new Dictionary<string, int>();

        return new AssessmentResponse(
            a.Id,
            a.SubmissionId,
            a.ReviewerUserId,
            a.TotalScore,
            a.Decision.ToString(),
            a.Comment,
            a.DecidedAt,
            scores
        );
    }
}
