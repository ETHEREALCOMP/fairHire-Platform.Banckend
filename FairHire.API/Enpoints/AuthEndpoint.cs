using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using Serilog;

namespace FairHire.API.Enpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        app.MapPost("/auth/sign-up", async (UserSignUpCommand command, SignUpRequest request,
            CancellationToken ct) =>
        {
            try 
            {
                var userId = await command.ExecuteAsync(request, ct);
                logger.Information("User signed up successfully with ID: {UserId}", userId.Id);
                return Results.Ok(userId);
            } 
            catch (Exception ex)
            {
                logger.Error("Error during sign-up: {Message}", ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }

        }).AllowAnonymous();

        app.MapPost("/auth/sign-in", async (UserSignInCommand command,
            SignInRequest request, CancellationToken ct) =>
        {
            try 
            {
                var user = await command.ExecuteAsync(request, ct);
                return Results.Ok(user);
            } 
            catch (Exception ex)
            {
                logger.Error("Error during sign-in: {Message}", ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }

        }).AllowAnonymous();
    }
}
