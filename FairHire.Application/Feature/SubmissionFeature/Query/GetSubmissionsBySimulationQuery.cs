using FairHire.Application.Feature.SubmissionFeature.Models.Response;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.SubmissionFeature.Query;

public sealed class GetSubmissionsBySimulationQuery(FairHireDbContext db)
{
    /// <summary>
    /// Лише компанія-власник симуляції.
    /// </summary>
    public async Task<IReadOnlyList<SubmissionResponse>> ExecuteAsync(Guid simulationId, 
        Guid callerId, CancellationToken ct)
    {
        var sim = await db.Simulations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == simulationId, ct)
            ?? throw new KeyNotFoundException("Simulation not found.");

        if (sim.CompanyId != callerId)
            throw new UnauthorizedAccessException("Access denied.");

        var list = await db.Submissions
            .AsNoTracking()
            .Where(x => x.SimulationId == simulationId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(s => new SubmissionResponse(
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
            ))
            .ToListAsync(ct);

        return list;
    }
}
