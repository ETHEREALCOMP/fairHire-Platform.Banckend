using FairHire.Domain;

namespace FairHire.Application.Feature.TestTaskFeature.Models.Responsess;

public class GetByIdTestTaskResponse
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public required string Status { get; set; }
    public Guid CreatedByCompanyId { get; set; }
    public CompanyProfile? CreatedByCompany { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
}
