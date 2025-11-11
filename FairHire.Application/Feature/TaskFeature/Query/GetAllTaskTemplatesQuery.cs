using FairHire.Application.Feature.TaskFeature.Models.Response;
using FairHire.Domain.Enums;
using FairHire.Domain.TaskLibrary;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TaskFeature.Query;

public sealed class GetAllTaskTemplatesQuery(FairHireDbContext db)
{
    /// <summary>
    /// callerCompanyId != null → список шаблонів компанії (з фільтрами q/status).
    /// null → кандидат/дев → лише Active; q ігнорується для безпеки (по NormalizedTitle теж фільтруємо).
    /// </summary>
    public async Task<IReadOnlyList<TaskTemplateResponse>> ExecuteAsync(
        Guid? callerCompanyId,
        string? q,
        string? status,
        CancellationToken ct)
    {
        IQueryable<TaskTemplate> query = db.TaskTemplates.AsNoTracking();

        if (callerCompanyId is Guid companyId)
        {
            query = query.Where(x => x.CreatedByCompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToUpperInvariant();
                query = query.Where(x => x.NormalizedTitle.Contains(k));
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<TemplateStatus>(status, true, out var st))
            {
                query = query.Where(x => x.Status == st);
            }
        }
        else
        {
            // Кандидат/дев: тільки Active
            query = query.Where(x => x.Status == TemplateStatus.Active);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToUpperInvariant();
                query = query.Where(x => x.NormalizedTitle.Contains(k));
            }
        }

        var list = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(t => new TaskTemplateResponse(
                t.Id,
                t.Title,
                t.NormalizedTitle,
                t.Description,
                t.Level,
                t.Tags != null ? t.Tags.ToList() : new List<string>(),
                t.EstimatedHours,
                t.Attachments != null ? t.Attachments.ToList() : new List<string>(),
                t.Status,
                t.CreatedAt
            ))
            .ToListAsync(ct);

        return list;
    }
}
