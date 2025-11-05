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
            app.MapPatch("/user/edit/{userId:guid}", async (Guid userId, EditUserCommand command,
            EditUserRequest request, CancellationToken ct) =>
            {
                await command.ExecuteAsync(userId, request, ct);
                return Results.Ok();
            }).RequireAuthorization(policy => policy.RequireRole("Developer", "Company"));

            app.MapGet("/user/get/{userId:guid}", async(Guid userId,
                GetUserDataQuery query, CancellationToken ct) => 
            {
                var res = await query.ExecuteAsync(userId, ct);
                return Results.Ok(res);
            }).RequireAuthorization(policy => policy.RequireRole("Developer", "Company"));
        }
    }
}
