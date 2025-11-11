using FairHire.API.Helpers;
using FairHire.Application.Feature.TaskFeature.Command;
using FairHire.Application.Feature.TaskFeature.Models.Request;
using FairHire.Application.Feature.TaskFeature.Query;
using FairHire.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FairHire.API.Enpoints;

public static class TaskTemplateEndpoints
{
    public static void MapTaskTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        // CREATE
        app.MapPost("/task-templates", async (
            CreateTaskTemplateCommand command,
            TaskTemplateCreateRequest request,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            if (!AuthHelpers.TryGetUserId(user, out var callerId))
                return Results.Unauthorized();

            // Переконуємось, що викликає компанія
            var isCompany = await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);
            if (!isCompany) return Results.Forbid();

            var result = await command.ExecuteAsync(request, ct);
            return Results.Ok(result);
        }).RequireAuthorization("Сompany");

        // UPDATE
        app.MapPatch("/task-templates/{templateId:guid}", async (
            Guid templateId,
            ClaimsPrincipal user,
            UpdateTaskTemplateCommand command,
            FairHireDbContext context,
            TaskTemplateUpdateRequest request,
            CancellationToken ct) =>
        {
            if (!AuthHelpers.TryGetUserId(user, out var callerId))
                return Results.Unauthorized();

            var isCompany = await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);
            if (!isCompany) return Results.Forbid();

            await command.ExecuteAsync(templateId, request, ct);
            return Results.NoContent();
        }).RequireAuthorization("Сompany");

        // ARCHIVE (soft-delete)
        app.MapPost("/task-templates/{templateId:guid}/archive", async (
            Guid templateId,
            ClaimsPrincipal user,
            FairHireDbContext context,
            ArchiveTaskTemplateCommand command,
            CancellationToken ct) =>
        {
            if (!AuthHelpers.TryGetUserId(user, out var callerId))
                return Results.Unauthorized();

            var isCompany = await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);
            if (!isCompany) return Results.Forbid();

            await command.ExecuteAsync(templateId, ct);
            return Results.NoContent();
        }).RequireAuthorization("Сompany");

        // HARD DELETE (опціонально)
        app.MapDelete("/task-templates/{templateId:guid}", async (
            Guid templateId,
            ClaimsPrincipal user,
            FairHireDbContext context,
            DeleteTaskTemplateCommand command,
            CancellationToken ct) =>
        {
            if (!AuthHelpers.TryGetUserId(user, out var callerId))
                return Results.Unauthorized();

            var isCompany = await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);
            if (!isCompany) return Results.Forbid();

            await command.ExecuteAsync(templateId, ct);
            return Results.NoContent();
        }).RequireAuthorization("Сompany");

        // GET BY ID
        app.MapGet("/task-templates/{templateId:guid}", async (
            Guid templateId,
            GetTaskTemplateByIdQuery query,
            ClaimsPrincipal user,
            FairHireDbContext context,
            CancellationToken ct) =>
        {
            // Дозвіл: компанія бачить свої; дев/кандидат — лише Active
            Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
            var isCompany = callerId != Guid.Empty && await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);

            var dto = await query.ExecuteAsync(templateId, isCompany ? callerId : null, ct);
            return Results.Ok(dto);
        }).RequireAuthorization("CanOrCompany");

        // LIST (для компанії — свої; для девів — тільки Active)
        app.MapGet("/task-templates/all", async (
            GetAllTaskTemplatesQuery query,
            ClaimsPrincipal user,
            FairHireDbContext context,
            string? q,
            string? status,
            CancellationToken ct) =>
        {
            Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId);
            var isCompany = callerId != Guid.Empty && await context.CompanyProfiles.AsNoTracking()
                .AnyAsync(c => c.UserId == callerId, ct);

            var result = await query.ExecuteAsync(isCompany ? callerId : null, q, status, ct);
            return Results.Ok(result);
        }).RequireAuthorization("CanOrCompany");
    }
}
