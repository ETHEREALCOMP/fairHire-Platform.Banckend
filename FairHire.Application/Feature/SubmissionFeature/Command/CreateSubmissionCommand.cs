using FairHire.Application.Base.Response;
using FairHire.Application.CurrentUser;
using FairHire.Application.Feature.SubmissionFeature.Models.Request;
using FairHire.Domain.Enums;
using FairHire.Domain.SubmissionsAndAssessments;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.SubmissionFeature.Command;

public sealed class CreateSubmissionCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task<IdResponse> ExecuteAsync(SubmissionCreateRequest req, CancellationToken ct)
    {
        if (!me.IsCandidate) throw new UnauthorizedAccessException();

        var sim = await db.Simulations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.SimulationId, ct)
            ?? throw new KeyNotFoundException("Simulation not found.");

        if (sim.CandidateUserId != me.UserId) throw new UnauthorizedAccessException();
        if (sim.Status != SimulationStatus.Active)
            throw new ValidationException("Simulation is not active.");

        if (string.IsNullOrWhiteSpace(req.RepoUrl) && req.FileId is null)
            throw new ValidationException("Provide RepoUrl or FileId.");

        if (req.WorkItemId is Guid wiId)
        {
            var ok = await db.SimulationWorkItems.AsNoTracking()
                .AnyAsync(x => x.Id == wiId && x.SimulationId == sim.Id, ct);
            if (!ok) throw new ValidationException("WorkItem does not belong to this simulation.");
        }

        var submission = new Submission
        {
            SimulationId = sim.Id,
            WorkItemId = req.WorkItemId,
            CandidateUserId = me.UserId,
            RepoUrl = req.RepoUrl?.Trim(),
            PullRequestUrl = req.PullRequestUrl?.Trim(),
            CommitSha = string.IsNullOrWhiteSpace(req.CommitSha) ? null : req.CommitSha.Trim(),
            FileId = req.FileId,
            Status = SubmissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync(ct);
        return new() { Id = submission.Id };
    }
}
