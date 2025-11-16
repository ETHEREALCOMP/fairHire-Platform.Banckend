using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;

namespace FairHire.API.Enpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/sign-up", async 
            (UserSignUpCommand command, 
            SignUpRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("AuthEndpoint.Sign-Up");
            try
            {
                var result = await command.ExecuteAsync(request, ct);
                logger.LogInformation("User signed up successfully with ID: {UserId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during sign-up: {Message}", ex.Message);
                throw;
            }
        }).AllowAnonymous();

        app.MapPost("/auth/sign-in", async 
            (UserSignInCommand command,
            ILoggerFactory loggerFactory,
            SignInRequest request, 
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("AuthEndpoint.Sign-In");
            try
            {
                var result = await command.ExecuteAsync(request, ct);
                logger.LogInformation("User signed in successfully with ID: {UserId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error during sign-in: {Message}", ex.Message);
                throw;
            }
        }).AllowAnonymous();
    }
}
