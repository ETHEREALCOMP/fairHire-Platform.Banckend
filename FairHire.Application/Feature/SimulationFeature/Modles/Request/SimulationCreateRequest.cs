namespace FairHire.Application.Feature.SimulationFeature.Modles.Request;

public sealed record SimulationCreateRequest(
    Guid CandidateUserId,
    string Name,
    DateTime StartUtc,
    DateTime EndUtc,
    string? BaseRepoUrl,
    string? BaseProjectRef,
    List<Guid>? TaskTemplateIds
);
