using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Jwt;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad;

public sealed class SignInCommand(JwtService jwtService, UserManager<User> userManager)
{
    public async Task<string> ExecuteAsync(SignInRequest request, CancellationToken ct)
    {
        var email = request.Email?.Trim();
        var user = await userManager.FindByEmailAsync(email!);
        if (user is null)
            throw new InvalidOperationException("Invalid email or password.");

        var ok = await userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
            throw new InvalidOperationException("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);

        // Обрана роль (опційно). Підтверджуємо, що користувач справді має її.
        string? activeRole = null;
        if (!string.IsNullOrWhiteSpace(request.DesiredRole))
        {
            if (!roles.Contains(request.DesiredRole, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Requested role is not assigned to the user.");

            activeRole = roles.First(r => string.Equals(r, request.DesiredRole, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            // Політика дефолту: company має пріоритет, інакше developer (налаштуй під себе)
            activeRole = roles.Contains("company", StringComparer.OrdinalIgnoreCase) ? "company" :
                         roles.Contains("developer", StringComparer.OrdinalIgnoreCase) ? "developer" : null;
        }

        return jwtService.IssueToken(user.Id, user.Email!, roles, activeRole);
    }
}
