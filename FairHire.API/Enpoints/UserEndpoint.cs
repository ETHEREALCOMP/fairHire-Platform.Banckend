using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Auth.Query;
using Serilog;

namespace FairHire.API.Enpoints;

public static class UserEndpoint
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app) 
    {
        var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        app.MapPatch("/user/{userId:guid}", async (Guid userId, UpdateUserCommand command,
        UpdateUserRequest request, CancellationToken ct) =>
        {
            try
            {
                var result = await command.ExecuteAsync(userId, request, ct);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.Error("Error updating user {UserId}: {ErrorMessage}", userId, ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }

        }).RequireAuthorization("CanOrCompany");


        app.MapGet("/user/{userId:guid}", async(Guid userId,
            GetUserDataQuery query, CancellationToken ct) => 
        {
            try
            {
                var result = await query.ExecuteAsync(userId, ct);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.Error("Error retrieving user data {UserId}: {ErrorMessage}", userId, ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }

        }).RequireAuthorization("CanOrCompany");
    }
}
