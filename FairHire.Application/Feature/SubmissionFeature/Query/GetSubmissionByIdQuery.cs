using FairHire.Application.Feature.SubmissionFeature.Models.Response;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.SubmissionFeature.Query;

public sealed class GetSubmissionByIdQuery(FairHireDbContext db)
{
    /// <summary>
    /// Дозвіл: компанія-власник симуляції або кандидат-власник сабміту.
    /// </summary>
    public async Task<SubmissionResponse> ExecuteAsync(Guid submissionId, Guid callerId, CancellationToken ct)
    {
        var s = await db.Submissions
            .AsNoTracking()
            .Include(x => x.Simulation)
            .FirstOrDefaultAsync(x => x.Id == submissionId, ct)
            ?? throw new KeyNotFoundException("Submission not found.");

        var isCompanyOwner = s.Simulation.CompanyId == callerId;
        var isCandidateOwner = s.CandidateUserId == callerId;

        if (!isCompanyOwner && !isCandidateOwner)
            throw new UnauthorizedAccessException("Access denied.");

        return new SubmissionResponse(
            s.Id,
            s.SimulationId,
            s.WorkItemId,
            s.CandidateUserId,
            s.RepoUrl,
            s.PullRequestUrl,
            s.CommitSha,
            s.FileId,
            s.Status.ToString(),
            s.CreatedAt,
            s.ReviewedAt
        );
    }
}
