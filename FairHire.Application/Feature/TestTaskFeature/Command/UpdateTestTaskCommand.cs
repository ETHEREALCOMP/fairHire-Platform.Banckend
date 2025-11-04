using FairHire.Application.Base.Response;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class UpdateTestTaskCommand(AppDbContext context,
    UserManager<User> userManager)
{
    public async Task<BaseResponse> ExecuteAsync(Guid taskId, UpdateTestTaskRequest request, CancellationToken ct) 
    {
        var companyProfile = await context.CompanyProfiles.Where(x => x.UserId == request.CreatedByCompanyId)
            .AsNoTracking().FirstOrDefaultAsync(ct) 
            ?? throw new KeyNotFoundException("Company profile not found.");

        var companyUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.CreatedByCompanyId, ct)
            ?? throw new KeyNotFoundException("Company user not found.");

        var companyRoles = await userManager.GetRolesAsync(companyUser);

        var isCompany = companyRoles.Any(r => string.Equals(r, "Company", StringComparison.OrdinalIgnoreCase));
        if (!isCompany)
            throw new ValidationException("Creator user does not have 'Company' role.");

        var task = await context.TestTasks.Where(x => x.Id == taskId).FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Test task not found.");

        task.Status = "Updated";
        task.DueDateUtc = DateTime.UtcNow;
        task.Description = string.IsNullOrWhiteSpace(request.Description) ? task.Description : request.Description;
        task.Title = string.IsNullOrWhiteSpace(request.Title) ? task.Title : request.Title;

        await context.SaveChangesAsync();


        return new() { Id = task.Id };
    } 
}
