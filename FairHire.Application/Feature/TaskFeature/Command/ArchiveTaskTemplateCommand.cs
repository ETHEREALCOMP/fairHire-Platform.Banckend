using FairHire.Application.CurrentUser;
using FairHire.Domain.Enums;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TaskFeature.Command;

public sealed class ArchiveTaskTemplateCommand(FairHireDbContext db, ICurrentUser me)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct)
    {
        if (!me.IsCompany) throw new UnauthorizedAccessException();
        var t = await db.TaskTemplates.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Template not found.");
        if (t.CreatedByCompanyId != me.UserId) throw new UnauthorizedAccessException();

        t.Status = TemplateStatus.Archived;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
