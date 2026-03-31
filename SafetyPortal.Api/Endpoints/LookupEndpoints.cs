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

        return app;
    }
}
