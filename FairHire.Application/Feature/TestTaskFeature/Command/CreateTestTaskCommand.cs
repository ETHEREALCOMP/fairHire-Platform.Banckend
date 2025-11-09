using FairHire.Application.Base.Response;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class CreateTestTaskCommand(
    AppDbContext context,
    UserManager<User> userManager)
{
    public async Task<BaseResponse> ExecuteAsync(Guid companyUserId,
        CreateTestTaskRequest request, CancellationToken ct)
    {
        // 1) Валід
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        var normalizedTitle = request.Title.Trim();
        var normalizedTitleKey = normalizedTitle.ToUpperInvariant();

        // 2) Витягуєм профіль компанії і перевіряєм чи він існує?
        var companyProfile = await context.CompanyProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == companyUserId, ct) ??
            throw new KeyNotFoundException("Company profile not found.");

        // 3) Якщо є виконавець — перевір існування та роль Developer
        Guid? assigneeId = null;
        if (request.AssignedToUserId is Guid devId && devId != Guid.Empty)
        {
            var assignee = await userManager.FindByIdAsync(devId.ToString())
                ?? throw new KeyNotFoundException("Assignee user not found.");

            var roles = await userManager.GetRolesAsync(assignee);
            var isDeveloper = roles.Any(r => string.Equals(r, "Developer", StringComparison.OrdinalIgnoreCase));
            
            if (!isDeveloper)
                throw new ValidationException("Assignee user must have 'Developer' role.");

            assigneeId = devId; // тільки FK
        }

        // 4) Антидубль у межах компанії
        var titleExists = await context.TestTasks.AsNoTracking()
            .AnyAsync(t => t.CreatedByCompanyId == companyProfile.UserId 
            && t.Title == normalizedTitle, ct);
        
        if (titleExists)
            throw new ValidationException("Task with the same title already exists for this company.");

        // 6) Створення
        var task = new TestTask
        {
            Title = normalizedTitle,
            NormalizedTitle = normalizedTitleKey,
            Description = request.Description,
            DueDateUtc = DateTime.UtcNow,
            Status = "New",
            CreatedByCompanyId = companyProfile.UserId,
            AssignedToUserId = assigneeId
        };

        // 7) Додаємо завдання до списку створення
        companyProfile.CreatedTasks.Add(task);
    
        context.TestTasks.Add(task);
        await context.SaveChangesAsync(ct);

        return new() { Id = task.Id };
    }
}