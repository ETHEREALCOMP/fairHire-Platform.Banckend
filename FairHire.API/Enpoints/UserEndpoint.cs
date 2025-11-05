using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Auth.Query;
using Microsoft.AspNetCore.Builder;

namespace FairHire.API.Enpoints
{
    public static class UserEndpoint
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app) 
        {
            app.MapPatch("/user/update/{userId:guid}", async (Guid userId, UpdateUserCommand command,
            UpdateUserRequest request, CancellationToken ct) =>
            {
                var id = await command.ExecuteAsync(userId, request, ct);
                return Results.Ok(id);
            }).RequireAuthorization(policy => policy.RequireRole("developer", "company"));//іноді ламається і треаб переписати з малої

            app.MapGet("/user/get/{userId:guid}", async(Guid userId,
                GetUserDataQuery query, CancellationToken ct) => 
            {
                var res = await query.ExecuteAsync(userId, ct);
                return Results.Ok(res);
            }).RequireAuthorization(policy => policy.RequireRole("developer", "company"));//іноді ламається і треаб переписати з малої
        }
    }
}
