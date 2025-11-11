using FairHire.Domain.Enums;
using FairHire.Domain.SubmissionsAndAssessments;

namespace FairHire.Domain.Simulations;

public sealed class Simulation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CompanyId { get; set; } // FK -> CompanyProfile.UserId
    public Guid CandidateUserId { get; set; } // FK -> User
    public required string Name { get; set; }

    public string? BaseRepoUrl { get; set; }
    public string? BaseProjectRef { get; set; }
    public string? ForkRepoUrl { get; set; }

    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public SimulationStatus Status { get; set; } = SimulationStatus.Scheduled;

    public string? MetricsJson { get; set; } // {commits,prs,avgReviewResponse,completed,totalScore}
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }

    public CompanyProfile Company { get; set; } = default!;
    public User Candidate { get; set; } = default!;
    public User CreatedByUser { get; set; } = default!;

    public List<SimulationWorkItem> WorkItems { get; set; } = new();
    public List<Submission> Submissions { get; set; } = new();
}
