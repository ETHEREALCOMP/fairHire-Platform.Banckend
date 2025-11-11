using FairHire.Application.Feature.TaskFeature.Models.Response;
using FairHire.Domain.Enums;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TaskFeature.Query;

public sealed class GetTaskTemplateByIdQuery(FairHireDbContext db)
{
    /// <summary>
    /// Якщо callerCompanyId != null → повертати тільки шаблони цієї компанії.
    /// Якщо null → вважати, що це кандидат/розробник і повертати тільки Active.
    /// </summary>
    public async Task<TaskTemplateResponse> ExecuteAsync(Guid templateId, Guid? callerCompanyId, 
        CancellationToken ct)
    {
        var t = await db.TaskTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == templateId, ct)
            ?? throw new KeyNotFoundException("Template not found.");

        if (callerCompanyId is Guid companyId)
        {
            if (t.CreatedByCompanyId != companyId)
                throw new UnauthorizedAccessException("Not your template.");
        }
        else
        {
            if (t.Status != TemplateStatus.Active)
                throw new UnauthorizedAccessException("Template is not public (Active).");
        }

        return new TaskTemplateResponse(
            t.Id,
            t.Title,
            t.NormalizedTitle,
            t.Description,
            t.Level,
            t.Tags?.ToList() ?? new List<string>(),
            t.EstimatedHours,
            t.Attachments?.ToList() ?? new List<string>(),
            t.Status,
            t.CreatedAt
        );
    }
}
