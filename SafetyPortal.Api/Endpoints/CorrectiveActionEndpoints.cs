using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.CorrectiveActions;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Services;

namespace SafetyPortal.Api.Endpoints;

public static class CorrectiveActionEndpoints
{
    public static IEndpointRouteBuilder MapCorrectiveActionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/corrective-actions").WithTags("CorrectiveActions").RequireAuthorization();

        // GET /api/corrective-actions?reportId=&status=
        group.MapGet("/", async (SafetyPortalDbContext db, int? reportId = null, string? status = null) =>
        {
            var query = db.CorrectiveActions
                .Include(x => x.Report)
                .Include(x => x.AssignedToUser)
                .AsQueryable();

            if (reportId.HasValue)
                query = query.Where(x => x.ReportId == reportId);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);

            var items = await query
                .OrderBy(x => x.DueDate)
                .Select(x => new CorrectiveActionSummaryDto(
                    x.Id,
                    x.ReportId,
                    x.Report.ReportNumber,
                    x.ActionTitle,
                    x.AssignedToUser.FullName,
                    x.DueDate,
                    x.Status,
                    x.PriorityLevel
                ))
                .ToListAsync();

            return Results.Ok(items);
        });

        // GET /api/corrective-actions/export — filtered Excel download
        group.MapGet("/export", async (SafetyPortalDbContext db, int? reportId = null, string? status = null) =>
        {
            var query = db.CorrectiveActions
                .Include(x => x.Report)
                .Include(x => x.AssignedToUser)
                .AsQueryable();

            if (reportId.HasValue)
                query = query.Where(x => x.ReportId == reportId);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);

            var rows = await query
                .OrderBy(x => x.DueDate)
                .Select(x => new CorrectiveActionExportRow
                {
                    ReportNumber = x.Report.ReportNumber,
                    ActionTitle  = x.ActionTitle,
                    AssignedTo   = x.AssignedToUser.FullName,
                    DueDate      = x.DueDate.ToString("dd/MM/yyyy"),
                    Priority     = x.PriorityLevel,
                    Status       = x.Status
                })
                .ToListAsync();

            var bytes = await ExcelOrCsvCreator.CreateExcel(rows, "corrective-actions", "Corrective Actions");
            if (bytes is null)
                return Results.BadRequest(new { error = "No data to export." });

            var fileName = $"corrective-actions_{DateTime.Today:yyyy-MM-dd}.xlsx";
            return Results.File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        });

        // POST /api/corrective-actions
        group.MapPost("/", async (CreateCorrectiveActionDto request, SafetyPortalDbContext db) =>
        {
            var action = new CorrectiveAction
            {
                ReportId = request.ReportId,
                ActionTitle = request.ActionTitle,
                ActionDescription = request.ActionDescription,
                AssignedToUserId = request.AssignedToUserId,
                DueDate = request.DueDate,
                Status = "Pending",
                PriorityLevel = request.PriorityLevel
            };

            db.CorrectiveActions.Add(action);
            await db.SaveChangesAsync();

            return Results.Created($"/api/corrective-actions/{action.Id}", new { action.Id });
        });

        // PUT /api/corrective-actions/{id}/status
        group.MapPut("/{id:int}/status", async (int id, UpdateActionStatusDto request, SafetyPortalDbContext db) =>
        {
            var action = await db.CorrectiveActions.FindAsync(id);
            if (action is null)
                return Results.NotFound();

            action.Status = request.Status;
            if (request.Status == "Completed")
                action.CompletedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /api/corrective-actions/{id}
        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var action = await db.CorrectiveActions.FindAsync(id);
            if (action is null)
                return Results.NotFound();

            db.CorrectiveActions.Remove(action);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}
