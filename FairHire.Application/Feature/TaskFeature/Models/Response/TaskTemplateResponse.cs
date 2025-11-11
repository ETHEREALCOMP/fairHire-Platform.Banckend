using FairHire.Domain.Enums;

namespace FairHire.Application.Feature.TaskFeature.Models.Response;

public sealed record TaskTemplateResponse(
    Guid Id,
    string Title,
    string NormalizedTitle,
    string? Description,
    string? Level,
    List<string> Tags,
    int? EstimatedHours,
    List<string> Attachments,
    TemplateStatus Status,
    DateTime CreatedAt
);
