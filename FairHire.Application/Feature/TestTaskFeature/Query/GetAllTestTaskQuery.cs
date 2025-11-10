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
            .SelectMany(c => c.CreatedTasks)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.DueDateUtc,
                t.Status,
                t.CreatedByCompanyId,
                t.AssignedToUserId
            })
            .ToListAsync(ct);

        if (companyProfile is null)
            throw new KeyNotFoundException("Company profile not found.");


        return null;
        //return new()
        //{
        //    CreatedTasks = companyProfile.CreatedTasks
        //};
    }
}
