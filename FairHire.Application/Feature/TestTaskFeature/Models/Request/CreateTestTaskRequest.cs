using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TestTaskFeature.Models.Requests;

public sealed record CreateTestTaskRequest
{
    public Guid? AssignedToUserId { get; init; }               // дев, кому призначаємо (може бути null)
    public  required string Title { get; init; }
    public string? Description { get; init; }
}
