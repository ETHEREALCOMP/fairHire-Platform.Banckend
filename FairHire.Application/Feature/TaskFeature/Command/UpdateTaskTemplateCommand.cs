using FairHire.Application.CurrentUser;
using FairHire.Application.Feature.TaskFeature.Models.Request;
using FairHire.Domain.Enums;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TaskFeature.Command;

public sealed class UpdateTaskTemplateCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task ExecuteAsync(Guid id, TaskTemplateUpdateRequest req, CancellationToken ct)
    {
        if (!me.IsCompany) throw new UnauthorizedAccessException();

        var t = await db.TaskTemplates.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Template not found.");
        if (t.CreatedByCompanyId != me.UserId) throw new UnauthorizedAccessException();

        if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

        var normalized = req.Title.Trim();
        var normalizedKey = normalized.ToUpperInvariant();

        var dup = await db.TaskTemplates.AsNoTracking()
            .AnyAsync(x => x.CreatedByCompanyId == t.CreatedByCompanyId &&
                           x.Id != t.Id &&
                           x.NormalizedTitle == normalizedKey, ct);
        if (dup) throw new ValidationException("Template with same title already exists.");

        if (!Enum.TryParse<TemplateStatus>(req.Status, true, out var s))
            throw new ValidationException("Invalid status.");

        t.Title = normalized;
        t.NormalizedTitle = normalizedKey;
        t.Description = req.Description;
        t.Level = req.Level;
        t.Tags = req.Tags ?? [];
        t.EstimatedHours = req.EstimatedHours;
        t.Attachments = req.Attachments ?? [];
        t.Status = s;
        t.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
