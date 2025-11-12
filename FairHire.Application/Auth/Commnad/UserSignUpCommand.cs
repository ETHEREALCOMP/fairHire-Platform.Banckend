using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Base.Response;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FairHire.Application.Auth.Commnad;

public sealed class UserSignUpCommand(UserManager<User> userManager, FairHireDbContext context)
{
    public async Task<IdResponse> ExecuteAsync(SignUpRequest request, CancellationToken ct)
    {
        // 1) Валідації вводу (базові)
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Password is required.");

        if (request.Password != request.ConfPassword)
            throw new ValidationException("Password and Confirm Password do not match.");

        var roleToAssign = NormalizeRole(request.Role);
        if (roleToAssign is null)
            throw new ValidationException("Role must be either 'Company' or 'Developer'.");

        // 2) Анти-гонка: перевірити користувача іменем та емейлом (UserName = Email)
        var email = request.Email.Trim();
        var normalized = email.ToUpperInvariant();

        if (await userManager.FindByEmailAsync(email) is not null 
            || await userManager.FindByNameAsync(email) is not null)
            throw new DuplicateNameException("User with this email or username already exists.");

        // 3) Готуємо об’єкт User (з явною нормалізацією)
        var user = new User
        {
            Name = request.Name?.Trim() ?? email,
            UserName = email,
            NormalizedUserName = normalized,
            Email = email,
            NormalizedEmail = normalized
        };

        // 4) Транзакція: Create → AddToRole → CreateProfile → Save
        await using var trx = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var create = await userManager.CreateAsync(user, request.Password);
            if (!create.Succeeded)
                throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));

            var addRole = await userManager.AddToRoleAsync(user, roleToAssign);
            if (!addRole.Succeeded)
                throw new InvalidOperationException(string.Join("; ", addRole.Errors.Select(e => e.Description)));

            // 5) Створення профілю
            if (roleToAssign == Roles.Company)
            {
                context.CompanyProfiles.Add(new CompanyProfile
                {
                    UserId = user.Id,
                    Name = request.CompanyName?.Trim() ?? user.Name,
                    Address = request.Address?.Trim(),
                    Website = request.Website?.Trim()
                });
            }
            else // Candidate
            {
                var stacks = (request.Skills ?? new List<string>())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                context.CandidateProfiles.Add(new CandidateProfile
                {
                    UserId = user.Id,
                    Stacks = stacks
                });
            }

            await context.SaveChangesAsync(ct);
            await trx.CommitAsync(ct);

            return new () { Id = user.Id };
        }
        catch
        {
            await trx.RollbackAsync(ct);
            throw;
        }
    }

    private static string? NormalizeRole(string? roleInput)
    {
        if (string.IsNullOrWhiteSpace(roleInput)) return null;

        var r = roleInput.Trim().ToLowerInvariant();
        return r switch
        {
            "company" => Roles.Company,   // канонічна назва з seed
            "candidate" => Roles.Candidate,
            _ => null
        };
    }

    private static class Roles
    {
        public const string Company = "Company";
        public const string Candidate = "Candidate";
    }
}
