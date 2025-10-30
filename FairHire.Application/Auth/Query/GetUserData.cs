using FairHire.Application.Auth.Models.Responsess;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Auth.Query;

public sealed class GetUserData(AppDbContext context)
{
    public async Task<UserData> ExecuteAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Include(u=> u.TestTasks)
            .Include(u => u.Skills)
            .Select(u => new UserData(
                u.Email,
                u.Name,
                u.Skills,
                u.TestTasks))
            .FirstOrDefaultAsync(ct);
        
        return user;
    }
}
