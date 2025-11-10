using FairHire.Application.Exceptions;
using FairHire.Application.Feature.TestTaskFeature.Models.Responsess;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Query;

public sealed class GetByIdTestTaskQuery(AppDbContext context)
{
    public async Task<GetByIdTestTaskResponse> ExecuteAsync(Guid taskId, CancellationToken ct) {
        
        var task = await context.TestTasks.Where(x=> x.Id == taskId && !x.IsDeleted)
            .AsNoTracking().FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Task not found");

        return new() { 
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDateUtc = task.DueDateUtc,
            Status = task.Status,
            CreatedByCompanyId = task.CreatedByCompanyId,
            CreatedByCompany = task.CreatedByCompany,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUser = task.AssignedToUser
        };
    }
}
