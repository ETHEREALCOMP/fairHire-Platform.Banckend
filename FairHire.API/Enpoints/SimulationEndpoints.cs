using FairHire.API.Helpers;
using FairHire.Application.Feature.SimulationFeature.Command;
using FairHire.Application.Feature.SimulationFeature.Modles.Request;
using FairHire.Application.Feature.SimulationFeature.Query;
using FairHire.Application.Feature.WorkItemFeature.Command;
using FairHire.Application.Feature.WorkItemFeature.Modles.Request;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FairHire.API.Enpoints;

public static class SimulationEndpoints
{
    public static void MapSimulationEndpoints(this IEndpointRouteBuilder app)
    {
        // CREATE Simulation
        app.MapPost("/simulations", async 
            (CreateSimulationCommand command,
            SimulationCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SimulationEndpoints.Create");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                var result = await command.ExecuteAsync(request, ct);
                logger.LogInformation("Simulation created successfully with ID: {SimulationId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // ACTIVATE
        app.MapPost("/simulations/{simulationId:guid}/activate", async 
            (Guid simulationId,
            ActivateSimulationCommand command,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SimulationEndpoints.Activate");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                await command.ExecuteAsync(simulationId, ct);
                logger.LogInformation("Simulation activated successfully with ID: {SimulationId}", simulationId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // FINISH
        app.MapPost("/simulations/{simulationId:guid}/finish", async 
            (Guid simulationId,
            FinishSimulationCommand command,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SimulationEndpoints.Finish");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompanyOrAdmin =
                    await context.CompanyProfiles.AsNoTracking().AnyAsync(c => c.UserId == callerId, ct)
                    || user.IsInRole("admin");
                if (!isCompanyOrAdmin) return Results.Forbid();

                await command.ExecuteAsync(simulationId, ct);
                logger.LogInformation("Simulation finished successfully with ID: {SimulationId}", simulationId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }).RequireAuthorization("CompanyOrAdmin");

        // GET BY ID
        app.MapGet("/simulations/{simulationId:guid}", async 
            (Guid simulationId,
            GetSimulationByIdQuery query,
            ClaimsPrincipal user,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SimulationEndpoints.Get");
            try
            {
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
                var result = await query.ExecuteAsync(simulationId, callerId, ct);
                logger.LogInformation("Simulation getted successfully with ID: {SimulationId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }).RequireAuthorization("CanOrCompany");

        // ADD WorkItem
        app.MapPost("/simulations/{simulationId:guid}/work-items", async 
            (Guid simulationId,
            CreateWorkItemCommand command,
            WorkItemCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SimulationEndpoints.Work-items.Create");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                var result = await command.ExecuteAsync(request with { SimulationId = simulationId }, ct);
                logger.LogInformation("Simulation work-Items created successfully with ID: {SimulationId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // UPDATE WorkItem Status
        app.MapPatch("/work-items/{workItemId:guid}/status", async (
            Guid workItemId,
            UpdateWorkItemStatusCommand command,
            WorkItemUpdateStatusRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("SimulationEndpoints.Work-Items.Status.Update");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                await command.ExecuteAsync(workItemId, request.Status, ct);
                logger.LogInformation("Simulation work-items status updated successfully with ID: {WorkItemId}", workItemId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");
    }
}
