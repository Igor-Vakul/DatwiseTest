using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Entities;
using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Endpoints;

public static class AdminStatusEndpoints
{
    public static IEndpointRouteBuilder MapAdminStatusEndpoints(this IEndpointRouteBuilder app)
    {
        MapIncidentStatuses(app);
        MapSeverityLevels(app);
        MapActionStatuses(app);
        return app;
    }

    // ── Incident Statuses ─────────────────────────────────────────────────

    private static void MapIncidentStatuses(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/incident-statuses")
            .WithTags("Admin - Statuses")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (SafetyPortalDbContext db) =>
        {
            var items = await db.IncidentStatusOptions
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
            return Results.Ok(items);
        });

        group.MapPost("/", async (IncidentStatusDto request, SafetyPortalDbContext db) =>
        {
            if (await db.IncidentStatusOptions.AnyAsync(x => x.Name == request.Name))
                return Results.Conflict(new { message = "Status name already exists." });

            var maxOrder = await db.IncidentStatusOptions.MaxAsync(x => (int?)x.DisplayOrder) ?? 0;
            var item = new IncidentStatusOption
            {
                Name = request.Name,
                IsClosing = request.IsClosing,
                Color = request.Color ?? "#6c757d",
                DisplayOrder = maxOrder + 1,
                IsActive = true,
                IsSystem = false
            };

            db.IncidentStatusOptions.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/incident-statuses/{item.Id}", new { item.Id });
        });

        group.MapPut("/{id:int}", async (int id, IncidentStatusDto request, SafetyPortalDbContext db) =>
        {
            var item = await db.IncidentStatusOptions.FindAsync(id);
            if (item is null) return Results.NotFound();

            if (await db.IncidentStatusOptions.AnyAsync(x => x.Name == request.Name && x.Id != id))
                return Results.Conflict(new { message = "Status name already exists." });

            item.Name = request.Name;
            item.IsClosing = request.IsClosing;
            item.Color = request.Color ?? item.Color;
            item.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var item = await db.IncidentStatusOptions.FindAsync(id);
            if (item is null) return Results.NotFound();
            if (item.IsSystem) return Results.BadRequest(new { message = "System statuses cannot be deleted." });

            var inUse = await db.IncidentReports.AnyAsync(x => x.StatusId == item.Id);
            if (inUse) return Results.BadRequest(new { message = "Status is in use by existing incidents." });

            db.IncidentStatusOptions.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    // ── Severity Levels ───────────────────────────────────────────────────

    private static void MapSeverityLevels(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/severity-levels")
            .WithTags("Admin - Statuses")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (SafetyPortalDbContext db) =>
        {
            var items = await db.SeverityLevelOptions
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
            return Results.Ok(items);
        });

        group.MapPost("/", async (SimpleOptionDto request, SafetyPortalDbContext db) =>
        {
            if (await db.SeverityLevelOptions.AnyAsync(x => x.Name == request.Name))
                return Results.Conflict(new { message = "Severity level name already exists." });

            var maxOrder = await db.SeverityLevelOptions.MaxAsync(x => (int?)x.DisplayOrder) ?? 0;
            var item = new SeverityLevelOption
            {
                Name = request.Name,
                Color = request.Color ?? "#6c757d",
                DisplayOrder = maxOrder + 1,
                IsActive = true,
                IsSystem = false
            };

            db.SeverityLevelOptions.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/severity-levels/{item.Id}", new { item.Id });
        });

        group.MapPut("/{id:int}", async (int id, SimpleOptionDto request, SafetyPortalDbContext db) =>
        {
            var item = await db.SeverityLevelOptions.FindAsync(id);
            if (item is null) return Results.NotFound();

            if (await db.SeverityLevelOptions.AnyAsync(x => x.Name == request.Name && x.Id != id))
                return Results.Conflict(new { message = "Severity level name already exists." });

            item.Name = request.Name;
            item.Color = request.Color ?? item.Color;
            item.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var item = await db.SeverityLevelOptions.FindAsync(id);
            if (item is null) return Results.NotFound();
            if (item.IsSystem) return Results.BadRequest(new { message = "System severity levels cannot be deleted." });

            var inUse = await db.IncidentReports.AnyAsync(x => x.SeverityLevelId == item.Id);
            if (inUse) return Results.BadRequest(new { message = "Severity level is in use by existing incidents." });

            db.SeverityLevelOptions.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    // ── Action Statuses ───────────────────────────────────────────────────

    private static void MapActionStatuses(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/action-statuses")
            .WithTags("Admin - Statuses")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (SafetyPortalDbContext db) =>
        {
            var items = await db.ActionStatusOptions
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
            return Results.Ok(items);
        });

        group.MapPost("/", async (ActionStatusDto request, SafetyPortalDbContext db) =>
        {
            if (await db.ActionStatusOptions.AnyAsync(x => x.Name == request.Name))
                return Results.Conflict(new { message = "Action status name already exists." });

            var maxOrder = await db.ActionStatusOptions.MaxAsync(x => (int?)x.DisplayOrder) ?? 0;
            var item = new ActionStatusOption
            {
                Name = request.Name,
                IsCompleted = request.IsCompleted,
                Color = request.Color ?? "#6c757d",
                DisplayOrder = maxOrder + 1,
                IsActive = true,
                IsSystem = false
            };

            db.ActionStatusOptions.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/action-statuses/{item.Id}", new { item.Id });
        });

        group.MapPut("/{id:int}", async (int id, ActionStatusDto request, SafetyPortalDbContext db) =>
        {
            var item = await db.ActionStatusOptions.FindAsync(id);
            if (item is null) return Results.NotFound();

            if (await db.ActionStatusOptions.AnyAsync(x => x.Name == request.Name && x.Id != id))
                return Results.Conflict(new { message = "Action status name already exists." });

            item.Name = request.Name;
            item.IsCompleted = request.IsCompleted;
            item.Color = request.Color ?? item.Color;
            item.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var item = await db.ActionStatusOptions.FindAsync(id);
            if (item is null) return Results.NotFound();
            if (item.IsSystem) return Results.BadRequest(new { message = "System action statuses cannot be deleted." });

            var inUse = await db.CorrectiveActions.AnyAsync(x => x.StatusId == item.Id);
            if (inUse) return Results.BadRequest(new { message = "Action status is in use by existing corrective actions." });

            db.ActionStatusOptions.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public record IncidentStatusDto(
    [Required, StringLength(50)] string Name,
    bool IsClosing,
    [StringLength(7)] string? Color = null,
    bool IsActive = true
);

public record ActionStatusDto(
    [Required, StringLength(50)] string Name,
    bool IsCompleted,
    [StringLength(7)] string? Color = null,
    bool IsActive = true
);

public record SimpleOptionDto(
    [Required, StringLength(50)] string Name,
    [StringLength(7)] string? Color = null,
    bool IsActive = true
);
