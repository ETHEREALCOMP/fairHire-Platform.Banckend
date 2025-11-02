using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Auth.Models.Responsess;
using FairHire.Application.Jwt;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Application.Auth.Commnad.Companies;

public sealed class CompanySignInCommand(JwtService jwtService, AppDbContext context)
{
    public async Task<SignInResponse> ExecuteAsync(SignInRequest request, CancellationToken ct)
    {
        var email = request.Email?.Trim();
        var user = await context.Companies.Where(x => x.UserEmail == email).FirstOrDefaultAsync(ct);
        if (user is null)
            throw new InvalidOperationException("Invalid email or password.");

        var ok = await context.Companies.Where(x => x.Password== request.Password).FirstOrDefaultAsync(ct)??
            throw new InvalidOperationException("Invalid email or password.");

        var roles = await context.Companies.Where(x => x.UserRole == request.Role).FirstOrDefaultAsync(ct);

        var token = jwtService.IssueToken(user.Id, user.UserEmail!, roles);

        return new() {Token = token, Id = user.Id };
    }
}
