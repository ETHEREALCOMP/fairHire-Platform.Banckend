namespace FairHire.Application.Feature.SubmissionFeature.Models.Request;

public sealed record SubmissionCreateRequest(
    Guid SimulationId,
    Guid? WorkItemId,
    string? RepoUrl,
    string? PullRequestUrl,
    string? CommitSha,
    Guid? FileId
);
