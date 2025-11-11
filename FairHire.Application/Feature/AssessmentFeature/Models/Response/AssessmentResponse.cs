namespace FairHire.Application.Feature.AssessmentFeature.Models.Response;

public sealed record AssessmentResponse(
    Guid Id, Guid SubmissionId, Guid ReviewerUserId,
    int TotalScore, string Decision, string? Comment, DateTime DecidedAt,
    Dictionary<string, int> Scores
);
