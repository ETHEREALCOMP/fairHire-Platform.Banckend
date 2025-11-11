using FairHire.Domain.Enums;

namespace FairHire.Domain.SubmissionsAndAssessments;

public sealed class Assessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubmissionId { get; set; }
    public Guid ReviewerUserId { get; set; }

    public required string ScoresJson { get; set; } // { CodeQuality:4, ProblemSolving:5, ... }
    public int TotalScore { get; set; } // 0..100, denormalized
    public AssessmentDecision Decision { get; set; }
    public string? Comment { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

    public Submission Submission { get; set; } = default!;
    public User Reviewer { get; set; } = default!;
}
