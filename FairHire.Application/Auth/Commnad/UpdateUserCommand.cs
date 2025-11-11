using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Base.Response;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FairHire.Application.Auth.Commnad
{
    public sealed class UpdateUserCommand(UserManager<User> userManager, FairHireDbContext context)
    {
        public async Task<IdResponse> ExecuteAsync(Guid userId, UpdateUserRequest request, CancellationToken ct)
        {
            // базові перевірки
            if (userId == Guid.Empty)
                throw new ValidationException("UserId is required.");

            var user = await context.Users
                .Include(u => u.CompanyProfile)
                .Include(u => u.CandidateProfile)
                .FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new KeyNotFoundException("User not found.");

            // ролі
            var roles = await userManager.GetRolesAsync(user);

            // оновити User
            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name.Trim();

            // оновити CompanyProfile (якщо є роль Company)
            if (roles.Any(r => string.Equals(r, "Company", StringComparison.OrdinalIgnoreCase)))
            {
                if (user.CompanyProfile is null)
                {
                    user.CompanyProfile = new CompanyProfile
                    {
                        UserId = user.Id,
                        Name = request.CompanyName?.Trim() ?? user.Name
                    };
                }

                user.CompanyProfile.Name = string.IsNullOrWhiteSpace(request.CompanyName) ? user.Name : request.CompanyName.Trim();
                user.CompanyProfile.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
                user.CompanyProfile.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();

            }
            // оновити CandidateProfile в іншому випадку
            else
            {
                if (user.CandidateProfile is null)
                {
                    user.CandidateProfile = new CandidateProfile
                    {
                        UserId = user.Id,
                        Stacks = []
                    };
                }

                if (request.Stacks is not null)
                {
                    user.CandidateProfile.Stacks = request.Stacks
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }

            await context.SaveChangesAsync(ct);

            return new () { Id = user.Id };
        }
    }
}