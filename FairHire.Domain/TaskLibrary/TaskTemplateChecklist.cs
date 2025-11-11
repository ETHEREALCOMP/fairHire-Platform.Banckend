namespace FairHire.Domain.TaskLibrary;

public sealed class TaskTemplateChecklist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskTemplateId { get; set; }
    public required string ItemText { get; set; }
    public bool IsRequired { get; set; } = true;
    public int Weight { get; set; } = 1;

    public TaskTemplate TaskTemplate { get; set; } = default!;
}
