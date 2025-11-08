using FairHire.Application.Base.Response;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class CreateTestTaskCommand(
    AppDbContext context,
    UserManager<User> userManager,
    IHttpContextAccessor http)
{
    public async Task<BaseResponse> ExecuteAsync(CreateTestTaskRequest request, CancellationToken ct)
    {
        // 1) Валід
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");
        var normalizedTitle = request.Title.Trim();

        // 2) Перевірка приналежності: хто викликає = та сама компанія
        var callerIdStr = http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? throw new ValidationException("Unauthorized.");

        if (!Guid.TryParse(callerIdStr, out var callerId))
            throw new ValidationException("Invalid caller.");

        if (callerId != request.CreatedByCompanyId)
            throw new ValidationException("You cannot create tasks for another company.");

        // 3) Витягуєм профіль компанії і перевіряєм чи він існує?
        var companyProfile = await context.CompanyProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.CreatedByCompanyId, ct);

        if (companyProfile is null)
            throw new KeyNotFoundException("Company profile not found.");

        // 4) Якщо є виконавець — перевір існування та роль Developer
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

        // 5) Антидубль у межах компанії
        var titleExists = await context.TestTasks.AsNoTracking()
            .AnyAsync(t => t.CreatedByCompanyId == request.CreatedByCompanyId && t.Title == normalizedTitle, ct);
        
        if (titleExists)
            throw new ValidationException("Task with the same title already exists for this company.");

        // 6) Створення
        var task = new TestTask
        {
            Title = normalizedTitle,
            Description = request.Description,
            DueDateUtc = DateTime.UtcNow,
            Status = "New",
            CreatedByCompanyId = request.CreatedByCompanyId,
            AssignedToUserId = assigneeId
        };

        // 7) Додаємо завдання до списку створення
        companyProfile.CreatedTasks.Add(task);
    
        context.TestTasks.Add(task);
        await context.SaveChangesAsync(ct);

        return new BaseResponse { Id = task.Id };
    }
}