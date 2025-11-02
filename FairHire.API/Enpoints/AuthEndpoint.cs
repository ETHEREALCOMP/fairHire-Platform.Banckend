using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Commnad.Companies;
using FairHire.Application.Auth.Commnad.Users;
using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Auth.Models.Request.Companies;
using FairHire.Application.Auth.Models.Request.Users;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.API.Enpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/user/sign-up", async (UserSignUpCommand command, UserSignUpRequest request,
            CancellationToken ct) =>
        {
            var userId = await command.ExecuteAsync(request, ct);
            return Results.Ok(userId);

        }).AllowAnonymous();

        app.MapPost("/auth/company/sign-up", async (CompanySignUpCommand command, CompanySignUpRequest request,
            CancellationToken ct) =>
        {
            var companyId = await command.ExecuteAsync(request, ct);
            return Results.Ok(companyId);

        }).AllowAnonymous();

        app.MapPost("/auth/sign-in", async (UserSignInCommand command,
            SignInRequest request, CancellationToken ct) =>
        {
            var user = await command.ExecuteAsync(request, ct);
            return Results.Ok(user);
        }).AllowAnonymous();
    }
}
