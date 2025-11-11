using FairHire.Application.Base.Response;
using FairHire.Application.CurrentUser;
using FairHire.Application.Feature.AssessmentFeature.Models.Request;
using FairHire.Domain.Enums;
using FairHire.Domain.SubmissionsAndAssessments;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.AssessmentFeature.Command;

public sealed class CreateAssessmentCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task<IdResponse> ExecuteAsync(AssessmentCreateRequest req, CancellationToken ct)
    {
        if (!me.IsCompany) throw new UnauthorizedAccessException();

        var submissions = await db.Submissions
            .Include(x => x.Simulation)
            .FirstOrDefaultAsync(x => x.Id == req.SubmissionId, ct)
            ?? throw new KeyNotFoundException("Submission not found.");

        if (submissions.Simulation.CompanyId != me.UserId) throw new UnauthorizedAccessException();
        if (submissions.Status == SubmissionStatus.Reviewed)
            throw new ValidationException("Submission already reviewed.");

        if (!Enum.TryParse<AssessmentDecision>(req.Decision, true, out var decision))
            throw new ValidationException("Invalid decision.");

        if (req.Scores is null || req.Scores.Count == 0)
            throw new ValidationException("Scores are required.");
        foreach (var v in req.Scores.Values)
            if (v < 0 || v > 5) throw new ValidationException("Score must be 0..5.");

        var avg = req.Scores.Values.Average();
        var total = (int)Math.Round(avg * 20, MidpointRounding.AwayFromZero); // 0..100

        var assessment = new Assessment
        {
            SubmissionId = submissions.Id,
            ReviewerUserId = me.UserId,
            ScoresJson = System.Text.Json.JsonSerializer.Serialize(req.Scores),
            TotalScore = total,
            Decision = decision,
            Comment = req.Comment,
            DecidedAt = DateTime.UtcNow
        };

        submissions.Status = SubmissionStatus.Reviewed;
        submissions.ReviewedAt = DateTime.UtcNow;

        db.Assessments.Add(assessment);
        await db.SaveChangesAsync(ct);
        return new() { Id = assessment.Id };
    }
}