using FairHire.Application.Feature.TestTaskFeature.Models.Response;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Queries;

public sealed class GetAllTestTaskQuery(AppDbContext context)
{
    public async Task<GetAllTestTaskResponse> ExecuteAsync(Guid companyId, CancellationToken ct)
    {
        var companyProfile = await context.CompanyProfiles
            .AsNoTracking()
            .Where(c => c.UserId == companyId)
            .Select(x => new
            {
                CreatedTasks = x.CreatedTasks.Select(task => new TestTask
                {
                    Title = task.Title,
                    Description = task.Description,
                    CreatedByCompanyId = companyId
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (companyProfile is null)
            throw new KeyNotFoundException("Company profile not found.");

        return new()
        {
            CreatedTasks = companyProfile.CreatedTasks
        };
    }
}
