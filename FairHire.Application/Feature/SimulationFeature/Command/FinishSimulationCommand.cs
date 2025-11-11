using FairHire.Application.CurrentUser;
using FairHire.Domain.Enums;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.SimulationFeature.Command;

public sealed class FinishSimulationCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task ExecuteAsync(Guid simulationId, CancellationToken ct)
    {
        if (!me.IsCompany && !me.IsAdmin) throw new UnauthorizedAccessException();
        var sim = await db.Simulations.FirstOrDefaultAsync(x => x.Id == simulationId, ct)
            ?? throw new KeyNotFoundException("Simulation not found.");
        if (!me.IsAdmin && sim.CompanyId != me.UserId) throw new UnauthorizedAccessException();

        sim.Status = SimulationStatus.Finished;
        await db.SaveChangesAsync(ct);
    }
}
