
using FairHire.Application.Auth.Models.Response;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Auth.Query;

public sealed class GetUserDataQuery(AppDbContext context, UserManager<User> userManager)
{
    public async Task<GetUserDataResponse?> ExecuteAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.CompanyProfile)
            .Include(u => u.DeveloperProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;

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
            DeveloperProfile = user.DeveloperProfile is null ? null : new GetUserDataResponse.Developer
            {
                Skills = user.DeveloperProfile.Skills.ToList()
            }
        };
    }
}
