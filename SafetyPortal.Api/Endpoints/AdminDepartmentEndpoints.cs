using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.Departments;
using SafetyPortal.Api.Entities;

namespace SafetyPortal.Api.Endpoints;

public static class AdminDepartmentEndpoints
{
    public static IEndpointRouteBuilder MapAdminDepartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/departments")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (SafetyPortalDbContext db) =>
        {
            var depts = await db.Departments
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.LocationName, x.Color, x.IsActive })
                .ToListAsync();
            return Results.Ok(depts);
        });

        group.MapPost("/", async (CreateDepartmentDto dto, SafetyPortalDbContext db) =>
        {
            if (await db.Departments.AnyAsync(x => x.Name == dto.Name))
                return Results.BadRequest(new { error = "Department name already exists." });

            var dept = new Department
            {
                Name         = dto.Name,
                LocationName = dto.LocationName,
                Color        = dto.Color,
                IsActive     = true
            };
            db.Departments.Add(dept);
            await db.SaveChangesAsync();
            return Results.Created($"/api/admin/departments/{dept.Id}",
                new { dept.Id, dept.Name, dept.LocationName, dept.Color, dept.IsActive });
        });

        group.MapPut("/{id:int}", async (int id, UpdateDepartmentDto dto, SafetyPortalDbContext db) =>
        {
            var dept = await db.Departments.FindAsync(id);
            if (dept is null) return Results.NotFound();

            if (await db.Departments.AnyAsync(x => x.Name == dto.Name && x.Id != id))
                return Results.BadRequest(new { error = "Department name already exists." });

            dept.Name         = dto.Name;
            dept.LocationName = dto.LocationName;
            dept.Color        = dto.Color;
            dept.IsActive     = dto.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapPut("/{id:int}/toggle-active", async (int id, SafetyPortalDbContext db) =>
        {
            var dept = await db.Departments.FindAsync(id);
            if (dept is null) return Results.NotFound();

            dept.IsActive = !dept.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok(new { dept.Id, dept.IsActive });
        });

        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var dept = await db.Departments.FindAsync(id);
            if (dept is null) return Results.NotFound();

            var hasOpen = await db.IncidentReports.AnyAsync(x =>
                x.DepartmentId == id &&
                (x.Status == "Open" || x.Status == "InProgress"));
            if (hasOpen)
                return Results.BadRequest(new { error = "Cannot delete department with open incidents." });

            db.Departments.Remove(dept);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}
