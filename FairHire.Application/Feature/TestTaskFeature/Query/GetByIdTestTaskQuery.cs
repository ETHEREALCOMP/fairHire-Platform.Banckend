using FairHire.Application.Feature.TestTaskFeature.Models.Responsess;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Query;

public sealed class GetByIdTestTaskQuery(AppDbContext context)
{
    public async Task<GetByIdTestTaskResponse> ExecuteAsync(Guid taskId, CancellationToken ct) {
        var task = await context.TestTasks
            .FirstOrDefaultAsync(u => u.Id == taskId, ct) ?? 
            throw new KeyNotFoundException($"Task with id: {taskId} was not found.");

        return new ()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDateUtc = task.DueDateUtc,
            Status = task.Status,
            CreatedByCompanyId = task.CreatedByCompanyId,
            AssignedToUserId = task.AssignedToUserId
        };
    }
}
