using FairHire.API.Helpers;
using FairHire.Application.Feature.TaskFeature.Command;
using FairHire.Application.Feature.TaskFeature.Models.Request;
using FairHire.Application.Feature.TaskFeature.Query;
using FairHire.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FairHire.API.Enpoints;

public static class TaskTemplateEndpoints
{
    public static void MapTaskTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        // CREATE
        app.MapPost("/task-templates", async 
            (CreateTaskTemplateCommand command,
            TaskTemplateCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("TaskTemplateEndpoints.Create");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                // Переконуємось, що викликає компанія
                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                var result = await command.ExecuteAsync(request, ct);
                logger.LogInformation("Task created successfully with ID: {TaskTemplateId}", result.Id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in Create task template endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // UPDATE
        app.MapPatch("/task-templates/{templateId:guid}", async 
            (Guid templateId,
            ClaimsPrincipal user,
            UpdateTaskTemplateCommand command,
            FairHireDbContext context,
            TaskTemplateUpdateRequest request,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("TaskTemplateEndpoints.Update");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                await command.ExecuteAsync(templateId, request, ct);
                logger.LogInformation("Task updated successfully with ID: {TaskTemplateId}", templateId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError("Error in Update task template endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // ARCHIVE (soft-delete)
        app.MapPost("/task-templates/{templateId:guid}/archive", async 
            (Guid templateId,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ArchiveTaskTemplateCommand command,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("TaskTemplateEndpoints.Archive");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                await command.ExecuteAsync(templateId, ct);
                logger.LogInformation("Task arhived successfully with ID: {TaskTemplateId}", templateId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError("Error in Archive task template endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // HARD DELETE (опціонально)
        app.MapDelete("/task-templates/{templateId:guid}", async 
            (Guid templateId,
            ClaimsPrincipal user,
            FairHireDbContext context,
            DeleteTaskTemplateCommand command,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("TaskTemplateEndpoints.Delete");
            try
            {
                if (!AuthHelpers.TryGetUserId(user, out var callerId))
                    return Results.Unauthorized();

                var isCompany = await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);
                if (!isCompany) return Results.Forbid();

                await command.ExecuteAsync(templateId, ct);
                logger.LogInformation("Task deleted successfully with ID: {TaskTemplateId}", templateId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError("Error in Delete task template endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("Company");

        // GET BY ID
        app.MapGet("/task-templates/{templateId:guid}", async 
            (Guid templateId,
            GetTaskTemplateByIdQuery query,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("TaskTemplateEndpoints.GetById");
            try
            {
                // Дозвіл: компанія бачить свої; дев/кандидат — лише Active
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
                var isCompany = callerId != Guid.Empty && await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);

                var result = await query.ExecuteAsync(templateId, isCompany ? callerId : null, ct);
                logger.LogInformation("Task Getted successfully with ID: {TaskTemplateId}", templateId);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in GetById task template endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("CanOrCompany");

        // LIST (для компанії — свої; для девів — тільки Active)
        app.MapGet("/task-templates/all", async 
            (GetAllTaskTemplatesQuery query,
            ClaimsPrincipal user,
            FairHireDbContext context,
            string? q,
            string? status,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("TaskTemplateEndpoints.All");
            try
            {
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
                var isCompany = callerId != Guid.Empty && await context.CompanyProfiles.AsNoTracking()
                    .AnyAsync(c => c.UserId == callerId, ct);

                var result = await query.ExecuteAsync(isCompany ? callerId : null, q, status, ct);
                logger.LogInformation("Task Getted successfully : {List}", result);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in List task templates endpoint: {Message}", ex.Message);
                throw;
            }
        }).RequireAuthorization("CanOrCompany");
    }
}
