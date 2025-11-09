using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class DeleteTestTaskCommand(AppDbContext context)
{
    public async Task ExecuteAsync(Guid companyUserId, Guid taskId, CancellationToken ct)
    {
        // 1) Витягуєм завдання
        var task = await context.TestTasks
            .FirstOrDefaultAsync(t => t.Id == taskId 
                && t.CreatedByCompanyId == companyUserId
                && !t.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Test task not found.");

        // 2) Логічне видалення
        task.IsDeleted = true;

        // 3) Збереження змін
        await context.SaveChangesAsync(ct);
    }
}
