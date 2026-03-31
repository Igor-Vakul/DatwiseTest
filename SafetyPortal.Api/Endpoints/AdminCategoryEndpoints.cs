using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.Categories;
using SafetyPortal.Api.Entities;

namespace SafetyPortal.Api.Endpoints;

public static class AdminCategoryEndpoints
{
    public static IEndpointRouteBuilder MapAdminCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/categories")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (SafetyPortalDbContext db) =>
        {
            var cats = await db.IncidentCategories
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.Description, x.IsActive })
                .ToListAsync();
            return Results.Ok(cats);
        });

        group.MapPost("/", async (CreateCategoryDto dto, SafetyPortalDbContext db) =>
        {
            if (await db.IncidentCategories.AnyAsync(x => x.Name == dto.Name))
                return Results.BadRequest(new { error = "Category name already exists." });

            var cat = new IncidentCategory { Name = dto.Name, Description = dto.Description, IsActive = true };
            db.IncidentCategories.Add(cat);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/categories/{cat.Id}",
                new { cat.Id, cat.Name, cat.Description, cat.IsActive });
        });

        group.MapPut("/{id:int}", async (int id, UpdateCategoryDto dto, SafetyPortalDbContext db) =>
        {
            var cat = await db.IncidentCategories.FindAsync(id);
            if (cat is null) return Results.NotFound();

            if (await db.IncidentCategories.AnyAsync(x => x.Name == dto.Name && x.Id != id))
                return Results.BadRequest(new { error = "Category name already exists." });

            cat.Name        = dto.Name;
            cat.Description = dto.Description;
            cat.IsActive    = dto.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapPut("/{id:int}/toggle-active", async (int id, SafetyPortalDbContext db) =>
        {
            var cat = await db.IncidentCategories.FindAsync(id);
            if (cat is null) return Results.NotFound();

            cat.IsActive = !cat.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok(new { cat.Id, cat.IsActive });
        });

        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var cat = await db.IncidentCategories.FindAsync(id);
            if (cat is null) return Results.NotFound();

            var hasOpen = await db.IncidentReports.AnyAsync(x =>
                x.CategoryId == id &&
                (x.Status == "Open" || x.Status == "InProgress"));
            if (hasOpen)
                return Results.BadRequest(new { error = "Cannot delete category with open incidents." });

            db.IncidentCategories.Remove(cat);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}
