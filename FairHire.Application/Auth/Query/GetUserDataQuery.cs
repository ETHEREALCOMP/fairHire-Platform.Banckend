using FairHire.Application.Auth.Models.Response;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Auth.Query;

public sealed class GetUserDataQuery(FairHireDbContext context, UserManager<User> userManager)
{
    public async Task<GetUserDataResponse?> ExecuteAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.CompanyProfile)
            .Include(u => u.CandidateProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, ct) ??
            throw new KeyNotFoundException($"User was not found."); ;

        var roles = await userManager.GetRolesAsync(user);

        return new GetUserDataResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name,
            Roles = roles.ToArray(),
            CompanyProfile = user.CompanyProfile is null ? null : new GetUserDataResponse.Company
            {
                Name = user.CompanyProfile.Name,
                Address = user.CompanyProfile.Address,
                Website = user.CompanyProfile.Website
            },
            CandidateProfile = user.CandidateProfile is null ? null : new GetUserDataResponse.Candidate
            {
                Stacks = user.CandidateProfile.Stacks.ToList()
            }
        };
    }
}
