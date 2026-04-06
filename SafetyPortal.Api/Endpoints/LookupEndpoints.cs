using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;

namespace SafetyPortal.Api.Endpoints;

public static class LookupEndpoints
{
    public static IEndpointRouteBuilder MapLookupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lookup").WithTags("Lookup").RequireAuthorization();

        group.MapGet("/departments", async (SafetyPortalDbContext db) =>
        {
            var depts = await db.Departments
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.LocationName, x.Color })
                .ToListAsync();
            return Results.Ok(depts);
        });

        group.MapGet("/categories", async (SafetyPortalDbContext db) =>
        {
            var cats = await db.IncidentCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.Description })
                .ToListAsync();
            return Results.Ok(cats);
        });

        group.MapGet("/users", async (SafetyPortalDbContext db) =>
        {
            var users = await db.Users
                .Include(x => x.Role)
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(x => new { x.Id, x.FullName, x.Email, RoleName = x.Role.Name })
                .ToListAsync();
            return Results.Ok(users);
        });

        group.MapGet("/roles", async (SafetyPortalDbContext db) =>
        {
            var roles = await db.Roles
                .OrderBy(x => x.Id)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();
            return Results.Ok(roles);
        });

        group.MapGet("/incident-statuses", async (SafetyPortalDbContext db) =>
        {
            var statuses = await db.IncidentStatusOptions
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new { x.Id, x.Name, x.IsClosing, x.Color })
                .ToListAsync();
            return Results.Ok(statuses);
        });

        group.MapGet("/severity-levels", async (SafetyPortalDbContext db) =>
        {
            var levels = await db.SeverityLevelOptions
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new { x.Id, x.Name, x.Color })
                .ToListAsync();
            return Results.Ok(levels);
        });

        group.MapGet("/action-statuses", async (SafetyPortalDbContext db) =>
        {
            var statuses = await db.ActionStatusOptions
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new { x.Id, x.Name, x.IsCompleted, x.Color })
                .ToListAsync();
            return Results.Ok(statuses);
        });

        return app;
    }
}
