using FairHire.Application.CurrentUser;
using FairHire.Domain.Enums;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.WorkItemFeature.Command;

public sealed class UpdateWorkItemStatusCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task ExecuteAsync(Guid workItemId, string newStatus, CancellationToken ct)
    {
        if (!me.IsCompany) throw new UnauthorizedAccessException();
        if (!Enum.TryParse<WorkItemStatus>(newStatus, true, out var st))
            throw new ValidationException("Invalid status.");

        var wi = await db.SimulationWorkItems.Include(x => x.Simulation)
            .FirstOrDefaultAsync(x => x.Id == workItemId, ct)
            ?? throw new KeyNotFoundException("WorkItem not found.");
        if (wi.Simulation.CompanyId != me.UserId) throw new UnauthorizedAccessException();

        wi.Status = st;
        if (st is WorkItemStatus.InReview or WorkItemStatus.Done)
            wi.CompletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
