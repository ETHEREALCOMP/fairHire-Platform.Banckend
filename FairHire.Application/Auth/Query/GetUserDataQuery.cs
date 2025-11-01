using FairHire.Application.Auth.Models.Responsess;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Auth.Query;

public sealed class GetUserDataQuery(UserManager<User> userManager, 
    AppDbContext context)
{
    public async Task<UserDataResponse> ExecuteAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        var data = await context.Users.Where(x => x.Id == user.Id)
            .Include(x => x.TestTasks)
            .Select(x => new UserDataResponse {
                Email = x.Email,
                Name = x.Name,
                Skills = x.Skills,
                TestTasks = x.TestTasks

            })
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);


        return data;
    }
}
