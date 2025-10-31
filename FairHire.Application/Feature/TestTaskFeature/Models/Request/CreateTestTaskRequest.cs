namespace FairHire.Application.Feature.TestTaskFeature.Models.Requests;

public sealed record CreateTestTaskRequest
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public Guid UserId { get; set; }

    public Guid CompanyId { get; set; }
}
