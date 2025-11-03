using FairHire.Application.Base.Response;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class CreateTestTaskCommand(AppDbContext context, 
    UserManager<User> userManager)
{
    public async Task<BaseResponse> ExecuteAsync(CreateTestTaskRequest request, 
        CancellationToken ct) 
    {
        // 1) Валідація базових полів
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required.");

        // 2) Перевірка, що компанійний профіль існує (CreatedByCompanyId = UserId компанії)
        var companyExists = await context.CompanyProfiles
            .AsNoTracking()
            .AnyAsync(c => c.UserId == request.CreatedByCompanyId, ct);

        if (!companyExists)
            throw new KeyNotFoundException("Company profile not found.");

        // (опційно) перевірка, що власник має роль Company
        var companyUser = await context.Users.FirstAsync(u => u.Id == request.CreatedByCompanyId, ct);
        var companyRoles = await userManager.GetRolesAsync(companyUser);
        if (!companyRoles.Contains("Company"))
            throw new ValidationException("Creator user does not have 'Company' role.");

        // 3) Якщо призначаємо деву — перевір, що користувач існує (і, за бажанням, що має роль Developer)
        User? assignee = null;
        if (request.AssignedToUserId is Guid devId)
        {
            assignee = await context.Users.FirstOrDefaultAsync(u => u.Id == devId, ct)
                ?? throw new KeyNotFoundException("Assignee user not found.");

            // (опційно) роль девелопера
            var roles = await userManager.GetRolesAsync(assignee);
            if (!roles.Contains("Developer"))
                throw new ValidationException("Assignee user does not have 'Developer' role.");
        }

        // 4) (опційно) Перевірка дубля по заголовку для цієї компанії
        var titleExists = await context.TestTasks.AsNoTracking()
            .AnyAsync(t => t.CreatedByCompanyId == request.CreatedByCompanyId && t.Title == request.Title, ct);
        if (titleExists)
            throw new ValidationException("Task with the same title already exists for this company.");

        // 5) Створюємо ентіті
        var task = new TestTask
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDateUtc = request.DueDateUtc,
            Status = "New",
            CreatedByCompanyId = request.CreatedByCompanyId,
            AssignedToUserId = request.AssignedToUserId
        };

        context.TestTasks.Add(task);
        await context.SaveChangesAsync(ct);

        return new BaseResponse { Id = task.Id };
    }
}
