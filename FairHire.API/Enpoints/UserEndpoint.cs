using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Auth.Query;

namespace FairHire.API.Enpoints;

public static class UserEndpoint
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app) 
    {
        app.MapPatch("/user/{userId:guid}", async 
            (Guid userId, 
            UpdateUserCommand command,
            UpdateUserRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("UserEndpoint.Update");
            try
            {
                var result = await command.ExecuteAsync(userId, request, ct);
                logger.LogInformation("User data updated successfully with ID: {UserId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error updating user {UserId}: {ErrorMessage}", userId, ex.Message);
                throw;
            }
        }).RequireAuthorization("CanOrCompany");


        app.MapGet("/user/{userId:guid}", async
            (Guid userId,
            GetUserDataQuery query,
            ILoggerFactory loggerFactory,
            CancellationToken ct) => 
        {
            var logger = loggerFactory.CreateLogger("UserEndpoint.Get");
            try
            {
                var result = await query.ExecuteAsync(userId, ct);
                logger.LogInformation("User data Getted successfully with ID: {UserId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error retrieving user data {UserId}: {ErrorMessage}", userId, ex.Message);
                throw;
            }
        }).RequireAuthorization("CanOrCompany");
    }
}
