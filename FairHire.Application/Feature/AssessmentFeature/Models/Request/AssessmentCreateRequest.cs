namespace FairHire.Application.Feature.AssessmentFeature.Models.Request;

public sealed record AssessmentCreateRequest(
    Guid SubmissionId,
    Dictionary<string, int> Scores,
    string Decision,
    string? Comment
);
