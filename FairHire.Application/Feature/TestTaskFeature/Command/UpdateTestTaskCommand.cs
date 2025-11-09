using FairHire.Application.Base.Response;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class UpdateTestTaskCommand(AppDbContext context)
{
    public async Task<BaseResponse> ExecuteAsync(Guid companyUserId, 
        Guid taskId, UpdateTestTaskRequest request, CancellationToken ct) 
    {
        var task = await context.TestTasks.Where(x => x.Id == taskId && 
        x.CreatedByCompanyId == companyUserId).FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Test task not found.");

        var normalizedTitle = request.Title.Trim();
        var normalizedTitleKey = normalizedTitle.ToUpperInvariant();

        task.DueDateUtc = request.DueDateUtc;

        task.Description = string.IsNullOrWhiteSpace(request.Description)
            ? task.Description : request.Description;

        task.Title = string.IsNullOrWhiteSpace(normalizedTitle) 
            ? task.Title : normalizedTitle;

        task.NormalizedTitle = string.IsNullOrWhiteSpace(normalizedTitleKey) 
            ? task.NormalizedTitle : normalizedTitleKey;

        await context.SaveChangesAsync(ct);

        return new() { Id = task.Id };
    } 
}
