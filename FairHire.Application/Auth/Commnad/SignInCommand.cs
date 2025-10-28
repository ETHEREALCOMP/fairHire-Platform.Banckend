using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Jwt;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad;

public sealed class SignInCommand(JwtService jwtService, UserManager<User> userManager)
{
    public async Task<string> ExecuteAsync(SignInRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }
        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        var token = jwtService.IssueToken(user.Id, user.Email);
        return token;
    }
}
