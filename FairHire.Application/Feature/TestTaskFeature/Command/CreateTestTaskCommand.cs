using FairHire.Application.Base.Response;
using FairHire.Application.Feature.TestTaskFeature.Models.Requests;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Commands;

public sealed class CreateTestTaskCommand(AppDbContext context, 
    UserManager<User> userManager)
{
    public async Task<BaseResponse> ExecuteAsync(CreateTestTaskRequest request, 
        CancellationToken ct) 
    {
        //var esitTask = await context.TestTasks.Where(x=> x.Title == request.Title)
        //    .SingleOrDefaultAsync(ct) ?? throw new Exception("Task already exist!");

        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        //var company = await context.Companies.Where(x=> x.Id == request.CompanyId)
        //    .FirstOrDefaultAsync(ct);

        var newTask = new TestTask
        {
            Title = request.Title,
            Description = request.Description,
            CompanyId = Guid.NewGuid(),
            UserId = request.UserId,
            User = user,
        };

        context.Add(newTask);
        await context.SaveChangesAsync();

        return new() { Id = newTask.Id };
    }
}
