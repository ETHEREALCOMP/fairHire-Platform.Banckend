using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TestTaskFeature.Models.Requests;

public sealed record CreateTestTaskRequest
{
    [Required] public Guid CreatedByCompanyId { get; init; }   // = CompanyProfile.UserId (userId компанії)
    public Guid? AssignedToUserId { get; init; }               // дев, кому призначаємо (може бути null)
    [Required, MaxLength(256)] public string Title { get; init; } = default!;
    [MaxLength(4000)] public string? Description { get; init; }
    public DateTime? DueDateUtc { get; init; }
}
