namespace FairHire.Application.Feature.SubmissionFeature.Models.Response;

public sealed record SubmissionResponse(
    Guid Id, Guid SimulationId, Guid? WorkItemId, Guid CandidateUserId,
    string? RepoUrl, string? PullRequestUrl, string? CommitSha, Guid? FileId,
    string Status, DateTime CreatedAt, DateTime? ReviewedAt
);
