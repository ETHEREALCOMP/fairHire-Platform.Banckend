using FairHire.Application.Base.Response;
using FairHire.Application.CurrentUser;
using FairHire.Application.Feature.WorkItemFeature.Modles.Request;
using FairHire.Domain.Enums;
using FairHire.Domain.Simulations;
using FairHire.Domain.TaskLibrary;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.WorkItemFeature.Command;

public sealed class CreateWorkItemCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task<IdResponse> ExecuteAsync(WorkItemCreateRequest req, CancellationToken ct)
    {
        if (!me.IsCompany) throw new UnauthorizedAccessException();

        var sim = await db.Simulations.Include(x => x.WorkItems)
            .FirstOrDefaultAsync(x => x.Id == req.SimulationId, ct)
            ?? throw new KeyNotFoundException("Simulation not found.");
        if (sim.CompanyId != me.UserId) throw new UnauthorizedAccessException();

        TaskTemplate? src = null;
        if (req.SourceTaskTemplateId is Guid tid)
        {
            src = await db.TaskTemplates.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tid && t.CreatedByCompanyId == me.UserId, ct)
                ?? throw new ValidationException("Template not found or not owned.");
        }

        var wi = new SimulationWorkItem
        {
            SimulationId = sim.Id,
            SourceTaskTemplateId = src?.Id,
            Title = src?.Title ?? req.Title,
            Description = src?.Description ?? req.Description,
            Level = src?.Level ?? req.Level,
            Tags = src?.Tags ?? (req.Tags ?? []),
            ChecklistJson = req.ChecklistJson,
            Status = WorkItemStatus.Backlog,
            Order = req.Order
        };

        sim.WorkItems.Add(wi);
        await db.SaveChangesAsync(ct);
        return new() { Id = wi.Id };
    }
}
