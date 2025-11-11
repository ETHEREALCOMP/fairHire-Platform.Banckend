using FairHire.Application.Feature.SimulationFeature.Modles.Response;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.SimulationFeature.Query;

public sealed class GetSimulationByIdQuery(FairHireDbContext db)
{
    /// <summary>
    /// Дозвіл: або компанія-власник, або кандидат-учасник.
    /// </summary>
    public async Task<SimulationResponse> ExecuteAsync(Guid simulationId, Guid callerId, CancellationToken ct)
    {
        var s = await db.Simulations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == simulationId, ct)
            ?? throw new KeyNotFoundException("Simulation not found.");

        var isCompanyOwner = s.CompanyId == callerId;
        var isCandidateOwner = s.CandidateUserId == callerId;

        if (!isCompanyOwner && !isCandidateOwner)
            throw new UnauthorizedAccessException("Access denied.");

        return new SimulationResponse(
            s.Id,
            s.CompanyId,
            s.CandidateUserId,
            s.Name,
            s.BaseRepoUrl,
            s.ForkRepoUrl,
            s.StartUtc,
            s.EndUtc,
            s.Status.ToString()
        );
    }
}
