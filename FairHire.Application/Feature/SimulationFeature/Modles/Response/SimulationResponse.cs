namespace FairHire.Application.Feature.SimulationFeature.Modles.Response;

public sealed record SimulationResponse(
    Guid Id, Guid CompanyId, Guid CandidateUserId, string Name,
    string? BaseRepoUrl, string? ForkRepoUrl,
    DateTime StartUtc, DateTime EndUtc, string Status
);
