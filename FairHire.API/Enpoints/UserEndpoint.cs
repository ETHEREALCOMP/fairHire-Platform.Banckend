using FairHire.Application.Auth.Commnad.Users;
using FairHire.Application.Auth.Models.Request.Users;
using FairHire.Application.Auth.Query;

namespace FairHire.API.Enpoints
{
    public static class UserEndpoint
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app) 
        {
            app.MapPatch("/user/edit/{userId:guid}", async (Guid userId, EditUserCommand command,
            EditUserDataRequest request, CancellationToken ct) =>
            {
                var res = await command.ExecuteAsync(userId, request, ct);
                return Results.Ok(res);
            }).RequireAuthorization("developer");

            app.MapGet("/user/get/{userId:guid}", async(Guid userId,
                GetUserDataQuery query, CancellationToken ct) => 
            {
                var res = await query.ExecuteAsync(userId, ct);
                return Results.Ok(res);
            }).RequireAuthorization("developer");
        }
    }
}
