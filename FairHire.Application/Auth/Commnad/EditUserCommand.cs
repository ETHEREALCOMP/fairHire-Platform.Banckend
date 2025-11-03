using FairHire.Application.Auth.Models.Request;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Auth.Commnad
{
    public sealed class EditUserCommand(UserManager<User> userManager, AppDbContext context)
    {
        public async Task ExecuteAsync(Guid userId, EditUserRequest request, CancellationToken ct)
        {
            // базові перевірки
            if (request.UserId == Guid.Empty)
                throw new ValidationException("UserId is required.");

            var user = await context.Users
                .Include(u => u.CompanyProfile)
                .Include(u => u.DeveloperProfile)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                ?? throw new KeyNotFoundException("User not found.");

            // ролі
            var roles = await userManager.GetRolesAsync(user);
            var isCompany = roles.Contains("Company");
            var isDeveloper = roles.Contains("Developer");

            // оновити User
            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name.Trim();

            // оновити CompanyProfile (якщо є роль Company)
            if (isCompany)
            {
                if (user.CompanyProfile is null)
                {
                    user.CompanyProfile = new CompanyProfile
                    {
                        UserId = user.Id,
                        Name = request.CompanyName?.Trim() ?? user.Name
                    };
                }

                if (request.CompanyName is not null)
                    user.CompanyProfile.Name = string.IsNullOrWhiteSpace(request.CompanyName) ? user.CompanyProfile.Name : request.CompanyName.Trim();

                if (request.Address is not null)
                    user.CompanyProfile.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

                if (request.Website is not null)
                    user.CompanyProfile.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
            }

            // оновити DeveloperProfile (якщо є роль Developer)
            if (isDeveloper)
            {
                if (user.DeveloperProfile is null)
                {
                    user.DeveloperProfile = new DeveloperProfile
                    {
                        UserId = user.Id,
                        Skills = []
                    };
                }

                if (request.Skills is not null)
                {
                    user.DeveloperProfile.Skills = request.Skills
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
