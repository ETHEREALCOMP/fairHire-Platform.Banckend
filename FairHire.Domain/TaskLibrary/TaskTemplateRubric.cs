namespace FairHire.Domain.TaskLibrary;

public sealed class TaskTemplateRubric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskTemplateId { get; set; }
    public required string CriteriaJson { get; set; } // jsonb: [{key,label,weight,hint}...]

    public TaskTemplate TaskTemplate { get; set; } = default!;
}
