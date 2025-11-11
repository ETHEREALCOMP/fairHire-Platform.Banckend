using FairHire.Application.Base.Response;
using FairHire.Application.CurrentUser;
using FairHire.Application.Feature.SimulationFeature.Modles.Request;
using FairHire.Domain.Enums;
using FairHire.Domain.Simulations;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.SimulationFeature.Command;

public sealed class CreateSimulationCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task<IdResponse> ExecuteAsync(SimulationCreateRequest req, CancellationToken ct)
    {
        if (!me.IsCompany) throw new UnauthorizedAccessException();

        var candidateExists = await db.Users.AsNoTracking()
            .AnyAsync(u => u.Id == req.CandidateUserId, ct);
        if (!candidateExists) throw new ValidationException("Candidate not found.");

        if (req.EndUtc <= req.StartUtc)
            throw new ValidationException("EndUtc must be > StartUtc.");

        var sim = new Simulation
        {
            CompanyId = me.UserId,
            CandidateUserId = req.CandidateUserId,
            Name = string.IsNullOrWhiteSpace(req.Name) ? "Simulation" : req.Name.Trim(),
            BaseRepoUrl = req.BaseRepoUrl,
            BaseProjectRef = req.BaseProjectRef,
            StartUtc = req.StartUtc,
            EndUtc = req.EndUtc,
            Status = SimulationStatus.Scheduled,
            CreatedByUserId = me.UserId,
            CreatedAt = DateTime.UtcNow
        };

        // копії з шаблонів
        if (req.TaskTemplateIds is { Count: > 0 })
        {
            var templates = await db.TaskTemplates
                .AsNoTracking()
                .Where(t => req.TaskTemplateIds.Contains(t.Id) && t.CreatedByCompanyId == me.UserId)
                .ToListAsync(ct);

            foreach (var t in templates)
            {
                sim.WorkItems.Add(new SimulationWorkItem
                {
                    SourceTaskTemplateId = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Level = t.Level,
                    Tags = t.Tags,
                    ChecklistJson = null,
                    Status = WorkItemStatus.Backlog,
                    Order = 0
                });
            }
        }

        db.Simulations.Add(sim);
        await db.SaveChangesAsync(ct);
        return new() { Id = sim.Id };
    }
}
