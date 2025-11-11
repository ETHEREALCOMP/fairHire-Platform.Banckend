using FairHire.Application.CurrentUser;
using FairHire.Domain.Enums;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.SimulationFeature.Command;

public sealed class ActivateSimulationCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task ExecuteAsync(Guid simulationId, CancellationToken ct)
    {
        if (!me.IsCompany) throw new UnauthorizedAccessException();
        var sim = await db.Simulations.FirstOrDefaultAsync(x => x.Id == simulationId, ct)
            ?? throw new KeyNotFoundException("Simulation not found.");
        if (sim.CompanyId != me.UserId) throw new UnauthorizedAccessException();
        if (sim.Status != SimulationStatus.Scheduled)
            throw new ValidationException("Only scheduled can be activated.");

        sim.Status = SimulationStatus.Active;
        await db.SaveChangesAsync(ct);
    }
}
