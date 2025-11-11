namespace FairHire.Application.Feature.WorkItemFeature.Modles.Request;

public sealed record WorkItemCreateRequest(
    Guid? SourceTaskTemplateId,
    string Title,
    string? Description,
    string? Level,
    List<string>? Tags,
    string? ChecklistJson,
    int Order,
    Guid SimulationId
);
