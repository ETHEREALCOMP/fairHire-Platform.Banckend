using FairHire.Application.Feature.TestTaskFeature.Models.Responsess;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Query;

public sealed class GetByIdTestTaskQuery(AppDbContext context, UserManager<User> userManager)
{
    public async Task<GetByIdTestTaskResponse> ExecuteAsync(Guid taskId, CancellationToken ct) {
        var task = await context.TestTasks
            .FirstOrDefaultAsync(u => u.Id == taskId, ct);
        
        if (task is null) return null;//додати exeption 



        return new GetByIdTestTaskResponse 
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDateUtc = task.DueDateUtc,
            Status = task.Status,
            CreatedByCompany = task.CreatedByCompany,
            CreatedByCompanyId = task.CreatedByCompanyId,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUser = task.AssignedToUser
        };
    }
}
