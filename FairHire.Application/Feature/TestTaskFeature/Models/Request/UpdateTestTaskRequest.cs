namespace FairHire.Application.Feature.TestTaskFeature.Models.Requests;

public sealed record UpdateTestTaskRequest
{
    public Guid CreatedByCompanyId { get; set; }

    public string? Title { get; set; }
    
    public string? Description { get; set; }

    public DateTime? DueDateUtc { get; set; }
}
