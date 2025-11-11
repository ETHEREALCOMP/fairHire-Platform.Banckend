namespace FairHire.Application.Feature.TaskFeature.Models.Request;

public sealed record TaskTemplateUpdateRequest(
    string Title,
    string? Description,
    string? Level,
    List<string>? Tags,
    int? EstimatedHours,
    List<string>? Attachments,
    string Status
);
