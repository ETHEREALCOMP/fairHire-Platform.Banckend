using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Auth.Query;

namespace FairHire.API.Enpoints
{
    public static class UserEndpoint
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app) 
        {
            app.MapPatch("/user{userId:guid}", async (Guid userId, UpdateUserCommand command,
            UpdateUserRequest request, CancellationToken ct) =>
            {
                var id = await command.ExecuteAsync(userId, request, ct);
                return Results.Ok(id);
            }).RequireAuthorization("devOrCompany");


            app.MapGet("/user{userId:guid}", async(Guid userId,
                GetUserDataQuery query, CancellationToken ct) => 
            {
                var res = await query.ExecuteAsync(userId, ct);
                return Results.Ok(res);
            }).RequireAuthorization("devOrCompany");
        }
    }
}
