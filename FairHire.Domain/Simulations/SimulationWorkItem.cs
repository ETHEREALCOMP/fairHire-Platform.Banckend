using FairHire.Domain.Enums;
using FairHire.Domain.SubmissionsAndAssessments;
using FairHire.Domain.TaskLibrary;

namespace FairHire.Domain.Simulations;

public sealed class SimulationWorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SimulationId { get; set; }
    public Guid? SourceTaskTemplateId { get; set; } // snapshot origin

    // Snapshot fields from template:
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Level { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? ChecklistJson { get; set; } // optional snapshot of checklist

    public WorkItemStatus Status { get; set; } = WorkItemStatus.Backlog;
    public DateTime? AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Order { get; set; } = 0;

    public Simulation Simulation { get; set; } = default!;
    public TaskTemplate? SourceTaskTemplate { get; set; }
    public List<Submission> Submissions { get; set; } = new();

}
