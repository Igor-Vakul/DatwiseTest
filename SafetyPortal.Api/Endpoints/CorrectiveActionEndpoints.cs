using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.CorrectiveActions;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Services;
using static SafetyPortal.Api.AppConstants;

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
                .Include(x => x.StatusOption)
                .AsQueryable();

            if (reportId.HasValue)
                query = query.Where(x => x.ReportId == reportId);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.StatusOption.Name == status);

            var items = await query
                .OrderBy(x => x.DueDate)
                .Select(x => new CorrectiveActionSummaryDto(
                    x.Id,
                    x.ReportId,
                    x.Report.ReportNumber,
                    x.ActionTitle,
                    x.AssignedToUser!.FullName,
                    x.DueDate,
                    x.StatusOption.Name,
                    x.PriorityLevel
                ))
                .ToListAsync();

            return Results.Ok(items);
        });

        // GET /api/corrective-actions/export
        group.MapGet("/export", async (SafetyPortalDbContext db, int? reportId = null, string? status = null) =>
        {
            var query = db.CorrectiveActions
                .Include(x => x.Report)
                .Include(x => x.AssignedToUser)
                .Include(x => x.StatusOption)
                .AsQueryable();

            if (reportId.HasValue)
                query = query.Where(x => x.ReportId == reportId);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.StatusOption.Name == status);

            var rows = await query
                .OrderBy(x => x.DueDate)
                .Select(x => new CorrectiveActionExportRow
                {
                    ReportNumber = x.Report.ReportNumber,
                    ActionTitle = x.ActionTitle,
                    AssignedTo = x.AssignedToUser!.FullName,
                    DueDate = x.DueDate.ToString("dd/MM/yyyy"),
                    Priority = x.PriorityLevel,
                    Status = x.StatusOption.Name
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
            var defaultStatus = await db.ActionStatusOptions
                .Where(s => s.IsActive && !s.IsCompleted)
                .OrderBy(s => s.DisplayOrder)
                .FirstOrDefaultAsync();
            if (defaultStatus is null)
                return Results.StatusCode(500);

            var action = new CorrectiveAction
            {
                ReportId = request.ReportId,
                ActionTitle = request.ActionTitle,
                ActionDescription = request.ActionDescription,
                AssignedToUserId = request.AssignedToUserId,
                DueDate = request.DueDate,
                StatusId = defaultStatus.Id,
                PriorityLevel = request.PriorityLevel
            };

            db.CorrectiveActions.Add(action);
            await db.SaveChangesAsync();

            return Results.Created($"/api/corrective-actions/{action.Id}", new { action.Id });
        })
        .RequireAuthorization("SupervisorOrAbove");

        // PUT /api/corrective-actions/{id}/status
        group.MapPut("/{id:int}/status", async (int id, UpdateActionStatusDto request, SafetyPortalDbContext db) =>
        {
            var action = await db.CorrectiveActions.FindAsync(id);
            if (action is null)
                return Results.NotFound();

            var statusOption = await db.ActionStatusOptions
                .FirstOrDefaultAsync(s => s.Name == request.Status);
            if (statusOption is null)
                return Results.BadRequest(new { error = "Invalid status." });

            action.StatusId = statusOption.Id;
            if (statusOption.IsCompleted)
                action.CompletedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization("SupervisorOrAbove");

        // DELETE /api/corrective-actions/{id}
        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var action = await db.CorrectiveActions.FindAsync(id);
            if (action is null)
                return Results.NotFound();

            db.CorrectiveActions.Remove(action);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization("SafetyManagerOrAdmin");

        return app;
    }
}
