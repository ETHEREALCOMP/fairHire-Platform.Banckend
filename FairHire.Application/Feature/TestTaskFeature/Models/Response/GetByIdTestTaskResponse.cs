using FairHire.Domain;

namespace FairHire.Application.Feature.TestTaskFeature.Models.Responsess;

public sealed class GetByIdTestTaskResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public DateTime? DueDateUtc { get; init; }
    public string Status { get; init; } = "New"; // New/InProgress/Done/Rejected/Updated

    // Хто створив (компанія)
    public Guid CreatedByCompanyId { get; init; }      // FK -> CompanyProfile.UserId

    // Кому призначено (дев-юзер)
    public Guid? AssignedToUserId { get; init; }        // FK -> AspNetUsers.Id
}
