using FairHire.Application.Exceptions;
using FairHire.Application.Feature.TestTaskFeature.Models.Response;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Feature.TestTaskFeature.Queries;

public sealed class GetAllTestTaskQuery(AppDbContext context)
{
    public async Task<List<GetAllTestTaskResponse>> ExecuteAsync(Guid companyId, CancellationToken ct)
    {
        var company = await context.CompanyProfiles
            .FirstOrDefaultAsync(x => x.UserId == companyId, ct) 
            ?? throw new NotFoundException("Company profile not found");
    
        var tasks = await context.TestTasks
            .Where(x => x.CreatedByCompanyId == company.UserId && !x.IsDeleted)
            .AsNoTracking()
            .Select(x => new GetAllTestTaskResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                DueDateUtc = x.DueDateUtc,
                Status = x.Status,
                CreatedByCompanyId = x.CreatedByCompanyId,
                CreatedByCompany = x.CreatedByCompany,
                AssignedToUserId = x.AssignedToUserId,
                AssignedToUser = x.AssignedToUser
            })
            .ToListAsync(ct);

        return tasks;
    }
}
