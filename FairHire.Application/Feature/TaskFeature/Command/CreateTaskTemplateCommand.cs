using FairHire.Application.Base.Response;
using FairHire.Application.CurrentUser;
using FairHire.Application.Feature.TaskFeature.Models.Request;
using FairHire.Domain.Enums;
using FairHire.Domain.TaskLibrary;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TaskFeature.Command
{
    public sealed class CreateTaskTemplateCommand(FairHireDbContext db, ICurrentUser me)
    {
        public async Task<IdResponse> ExecuteAsync(TaskTemplateCreateRequest req, CancellationToken ct)
        {
            if (!me.IsCompany) throw new UnauthorizedAccessException();
            if (string.IsNullOrWhiteSpace(req.Title)) throw new ValidationException("Title is required.");

            var companyExists = await db.CompanyProfiles.AsNoTracking()
                .AnyAsync(x => x.UserId == me.UserId, ct);
            if (!companyExists) throw new ValidationException("Company profile not found.");

            var normalized = req.Title.Trim();
            var normalizedKey = normalized.ToUpperInvariant();

            var dup = await db.TaskTemplates.AsNoTracking()
                .AnyAsync(t => t.CreatedByCompanyId == me.UserId && t.NormalizedTitle == normalizedKey, ct);
            if (dup) throw new ValidationException("Template with same title already exists.");

            var entity = new TaskTemplate
            {
                CreatedByCompanyId = me.UserId,
                Title = normalized,
                NormalizedTitle = normalizedKey,
                Description = req.Description,
                Level = req.Level,
                Tags = req.Tags ?? [],
                EstimatedHours = req.EstimatedHours,
                Attachments = req.Attachments ?? [],
                Status = TemplateStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            db.TaskTemplates.Add(entity);
            await db.SaveChangesAsync(ct);
            return new() { Id = entity.Id };
        }
    }
}
