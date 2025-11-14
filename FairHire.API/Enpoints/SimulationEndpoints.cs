using FairHire.API.Helpers;
using FairHire.Application.Feature.SimulationFeature.Command;
using FairHire.Application.Feature.SimulationFeature.Modles.Request;
using FairHire.Application.Feature.SimulationFeature.Query;
using FairHire.Application.Feature.WorkItemFeature.Command;
using FairHire.Application.Feature.WorkItemFeature.Modles.Request;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace FairHire.API.Enpoints;

public static class SimulationEndpoints
{
    public static void MapSimulationEndpoints(this IEndpointRouteBuilder app)
    {
        var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        // CREATE Simulation
        app.MapPost("/simulations", async (
            CreateSimulationCommand command,
            SimulationCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            try 
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                var result = await command.ExecuteAsync(request, ct);
                return Results.Ok(result);
            } 
            catch (Exception ex)
            {
                logger.Error("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }
        }).RequireAuthorization("Company");

        // ACTIVATE
        app.MapPost("/simulations/{simulationId:guid}/activate", async (
            Guid simulationId,
            ActivateSimulationCommand command,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                await command.ExecuteAsync(simulationId, ct);
                return Results.NoContent();
            } 
            catch (Exception ex)
            {
                logger.Error("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }

        }).RequireAuthorization("Company");

        // FINISH
        app.MapPost("/simulations/{simulationId:guid}/finish", async (
            Guid simulationId,
            FinishSimulationCommand command,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            try 
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompanyOrAdmin =
                    await context.CompanyProfiles.AsNoTracking().AnyAsync(c => c.UserId == callerId, ct)
                    || user.IsInRole("admin");
                if (!isCompanyOrAdmin) return Results.Forbid();

                await command.ExecuteAsync(simulationId, ct);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }

        }).RequireAuthorization("CompanyOrAdmin");

        // GET BY ID
        app.MapGet("/simulations/{simulationId:guid}", async (
            Guid simulationId,
            GetSimulationByIdQuery query,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            try 
            {
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
                var result = await query.ExecuteAsync(simulationId, callerId, ct);
                return Results.Ok(result);
            } 
            catch (Exception ex)
            {
                logger.Error("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }

        }).RequireAuthorization("CanOrCompany");

        // ADD WorkItem
        app.MapPost("/simulations/{simulationId:guid}/work-items", async (
            Guid simulationId,
            CreateWorkItemCommand command,
            WorkItemCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            try 
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                var result = await command.ExecuteAsync(request with { SimulationId = simulationId }, ct);
                return Results.Ok(result);
            } 
            catch(Exception ex) 
            {
                logger.Error("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            } 
            finally 
            {
                logger.Dispose();
            }

        }).RequireAuthorization("Company");

        // UPDATE WorkItem Status
        app.MapPatch("/work-items/{workItemId:guid}/status", async (
            Guid workItemId,
            UpdateWorkItemStatusCommand command,
            WorkItemUpdateStatusRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            try 
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                await command.ExecuteAsync(workItemId, request.Status, ct);
                return Results.NoContent();
            } 
            catch (Exception ex)
            {
                logger.Error("An error occurred: {ErrorMessage}", ex.Message);
                throw;
            }
            finally
            {
                logger.Dispose();
            }
        }).RequireAuthorization("Company");
    }
}
