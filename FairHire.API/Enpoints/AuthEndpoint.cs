using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using Microsoft.AspNetCore.Identity;

namespace FairHire.API.Enpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/sign-up", async (SignUpCommand command, SignUpRequest request,
            CancellationToken ct) =>
        {
            var userId = await command.ExecuteAsync(request, ct);
            return Results.Ok(userId);

        }).AllowAnonymous();

        app.MapPost("/auth/sign-in", async (SignInCommand command,
            SignInRequest request, CancellationToken ct) =>
        {
            var user = await command.ExecuteAsync(request, ct);
            return Results.Ok(user);
        }).AllowAnonymous();
    }
}
