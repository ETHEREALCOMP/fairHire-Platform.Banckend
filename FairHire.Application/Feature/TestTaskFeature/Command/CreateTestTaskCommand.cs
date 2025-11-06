using FairHire.Application.Base.Response;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class CreateTestTaskCommand(AppDbContext context, UserManager<User> userManager)
{
    public async Task<BaseResponse> ExecuteAsync(CreateTestTaskRequest request, CancellationToken ct)
    {
        // 1) Базова валідація
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        // Нормалізуємо заголовок один раз (щоб антидубль працював стабільно)
        var normalizedTitle = request.Title.Trim();

        // 2) Переконайся, що компанійний профіль існує (CreatedByCompanyId = UserId компанії)
        var companyProfile = await context.CompanyProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.CreatedByCompanyId, ct)
            ?? throw new KeyNotFoundException("Company profile not found.");

        // 3) Якщо призначаємо девелопера — перевірити існування та роль Developer (без регістру)
        if (request.AssignedToUserId is Guid devId)
        {
            var assignee = await context.Users
                .FirstOrDefaultAsync(u => u.Id == devId, ct)
                ?? throw new KeyNotFoundException("Assignee user not found.");

            var roles = await userManager.GetRolesAsync(assignee);
            var isDeveloper = roles.Any(r => string.Equals(r, "Developer", StringComparison.OrdinalIgnoreCase));
            if (!isDeveloper)
                throw new ValidationException("Assignee user does not have 'Developer' role.");
        }

        // 4) Антидубль по назві в межах компанії
        var titleExists = await context.TestTasks.AsNoTracking()
            .AnyAsync(t => t.CreatedByCompanyId == request.CreatedByCompanyId && t.Title == normalizedTitle, ct);
        if (titleExists)
            throw new ValidationException("Task with the same title already exists for this company.");

        // 5) Створити ентіті (DueDate — якщо прийде з реквесту; інакше залишити null)
        var task = new TestTask
        {
            Title = normalizedTitle,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDateUtc = DateTime.UtcNow, // якщо у твоєму CreateTestTaskRequest це поле є; інакше можеш лишити null
            Status = "New",
            CreatedByCompanyId = request.CreatedByCompanyId,
            AssignedToUserId = request.AssignedToUserId
        };

        context.TestTasks.Add(task);
        await context.SaveChangesAsync(ct);

        return new BaseResponse { Id = task.Id };
    }
}