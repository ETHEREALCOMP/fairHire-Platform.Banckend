using FairHire.Domain.Enums;
using FairHire.Domain.Infra;
using FairHire.Domain.Simulations;

namespace FairHire.Domain.SubmissionsAndAssessments;

public sealed class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SimulationId { get; set; }
    public Guid? WorkItemId { get; set; }
    public Guid CandidateUserId { get; set; }

    public string? RepoUrl { get; set; }
    public string? PullRequestUrl { get; set; }
    public string? CommitSha { get; set; }
    public Guid? FileId { get; set; } // ZIP (FileObject)

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Simulation Simulation { get; set; } = default!;
    public SimulationWorkItem? WorkItem { get; set; }
    public User Candidate { get; set; } = default!;
    public FileObject? File { get; set; }
    public Assessment? Assessment { get; set; } // 1:1 MVP
}
