using System.Security.Claims;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.CorrectiveActions;
using SafetyPortal.Api.Dtos.Incidents;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Jobs;
using SafetyPortal.Api.Services;

namespace SafetyPortal.Api.Endpoints;

public static class IncidentEndpoints
{
    public static IEndpointRouteBuilder MapIncidentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/incidents").WithTags("Incidents").RequireAuthorization();

        // GET /api/incidents — list with search + filters + pagination
        group.MapGet("/", async (
            SafetyPortalDbContext db,
            int page = AppConstants.Pagination.DefaultPage,
            int pageSize = AppConstants.Pagination.DefaultPageSize,
            string? search = null,
            string? status = null,
            string? severityLevel = null,
            int? departmentId = null,
            int? categoryId = null) =>
        {
            var query = db.IncidentReports
                .Include(x => x.Category)
                .Include(x => x.Department)
                .Include(x => x.ReportedByUser)
                .Include(x => x.CorrectiveActions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    x.ReportNumber.Contains(search) ||
                    x.Description.Contains(search));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(severityLevel))
                query = query.Where(x => x.SeverityLevel == severityLevel);

            if (departmentId.HasValue)
                query = query.Where(x => x.DepartmentId == departmentId);

            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.ReportedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new IncidentSummaryDto(
                    x.Id,
                    x.ReportNumber,
                    x.Title,
                    x.Category.Name,
                    x.Department.Name,
                    x.ReportedByUser.FullName,
                    x.IncidentDate,
                    x.SeverityLevel,
                    x.Status,
                    x.CorrectiveActions.Count
                ))
                .ToListAsync();

            return Results.Ok(new { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize });
        });

        // GET /api/incidents/export — filtered Excel download
        group.MapGet("/export", async (
            SafetyPortalDbContext db,
            string? search = null,
            string? status = null,
            string? severityLevel = null,
            int? departmentId = null,
            int? categoryId = null) =>
        {
            var query = db.IncidentReports
                .Include(x => x.Category)
                .Include(x => x.Department)
                .Include(x => x.ReportedByUser)
                .Include(x => x.AssignedToUser)
                .Include(x => x.CorrectiveActions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    x.ReportNumber.Contains(search) ||
                    x.Description.Contains(search));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(severityLevel))
                query = query.Where(x => x.SeverityLevel == severityLevel);

            if (departmentId.HasValue)
                query = query.Where(x => x.DepartmentId == departmentId);

            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId);

            var rows = await query
                .OrderByDescending(x => x.ReportedAt)
                .Select(x => new IncidentExportRow
                {
                    ReportNumber           = x.ReportNumber,
                    Title                  = x.Title,
                    CategoryName           = x.Category.Name,
                    DepartmentName         = x.Department.Name,
                    ReportedBy             = x.ReportedByUser.FullName,
                    AssignedTo             = x.AssignedToUser != null ? x.AssignedToUser.FullName : "",
                    IncidentDate           = x.IncidentDate,
                    SeverityLevel          = x.SeverityLevel,
                    Status                 = x.Status,
                    Location               = x.LocationDetails ?? "",
                    CorrectiveActionsCount = x.CorrectiveActions.Count
                })
                .ToListAsync();

            var bytes = await ExcelOrCsvCreator.CreateExcel(rows, "incidents", "Incidents");
            if (bytes is null)
                return Results.BadRequest(new { error = "No data to export." });

            var fileName = $"incidents_{DateTime.Today:yyyy-MM-dd}.xlsx";
            return Results.File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        });

        // GET /api/incidents/{id}
        group.MapGet("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var incident = await db.IncidentReports
                .Include(x => x.Category)
                .Include(x => x.Department)
                .Include(x => x.ReportedByUser)
                .Include(x => x.AssignedToUser)
                .Include(x => x.CorrectiveActions)
                    .ThenInclude(ca => ca.AssignedToUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (incident is null)
                return Results.NotFound();

            var dto = new IncidentDetailDto(
                incident.Id,
                incident.ReportNumber,
                incident.Title,
                incident.Description,
                incident.CategoryId,
                incident.Category.Name,
                incident.DepartmentId,
                incident.Department.Name,
                incident.ReportedByUserId,
                incident.ReportedByUser.FullName,
                incident.AssignedToUserId,
                incident.AssignedToUser?.FullName,
                incident.IncidentDate,
                incident.ReportedAt,
                incident.LocationDetails,
                incident.SeverityLevel,
                incident.Status,
                incident.CorrectiveActions.Select(ca => new CorrectiveActionSummaryDto(
                    ca.Id,
                    ca.ReportId,
                    incident.ReportNumber,
                    ca.ActionTitle,
                    ca.AssignedToUser.FullName,
                    ca.DueDate,
                    ca.Status,
                    ca.PriorityLevel
                )).ToList()
            );

            return Results.Ok(dto);
        });

        // POST /api/incidents
        group.MapPost("/", async (
            CreateIncidentDto    request,
            ClaimsPrincipal      user,
            SafetyPortalDbContext db,
            IBackgroundJobClient jobs) =>
        {
            if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Results.Unauthorized();

            var count        = await db.IncidentReports.CountAsync();
            var reportNumber = $"INC-{DateTime.UtcNow:yyyy}-{(count + 1):D4}";

            var incident = new IncidentReport
            {
                ReportNumber     = reportNumber,
                Title            = request.Title,
                Description      = request.Description,
                CategoryId       = request.CategoryId,
                DepartmentId     = request.DepartmentId,
                ReportedByUserId = userId,
                AssignedToUserId = request.AssignedToUserId,
                IncidentDate     = DateTime.Parse(request.IncidentDate),
                ReportedAt       = DateTime.UtcNow,
                LocationDetails  = request.LocationDetails,
                SeverityLevel    = request.SeverityLevel,
                Status           = "Open"
            };

            db.IncidentReports.Add(incident);
            await db.SaveChangesAsync();

            // Scenario 1 — fire-and-forget: send notification email in background
            jobs.Enqueue<IncidentNotificationJob>(j => j.SendCreatedAsync(incident.Id));

            // Scenario 3 — delayed escalation: if still Open after N days, alert managers
            jobs.Schedule<IncidentEscalationJob>(
                j => j.CheckAndEscalateAsync(incident.Id),
                TimeSpan.FromDays(AppConstants.Jobs.EscalationAfterDays));

            return Results.Created($"/api/incidents/{incident.Id}", new { incident.Id, incident.ReportNumber });
        });

        // PUT /api/incidents/{id}
        group.MapPut("/{id:int}", async (
            int                  id,
            UpdateIncidentDto    request,
            SafetyPortalDbContext db,
            IBackgroundJobClient jobs) =>
        {
            var incident = await db.IncidentReports
                .Include(x => x.ReportedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (incident is null)
                return Results.NotFound();

            incident.Title            = request.Title;
            incident.Description      = request.Description;
            incident.CategoryId       = request.CategoryId;
            incident.DepartmentId     = request.DepartmentId;
            incident.IncidentDate     = DateTime.Parse(request.IncidentDate);
            incident.LocationDetails  = request.LocationDetails;
            incident.SeverityLevel    = request.SeverityLevel;
            incident.Status           = request.Status;
            incident.AssignedToUserId = request.AssignedToUserId;

            await db.SaveChangesAsync();

            // Scenario 1 — fire-and-forget: send update notification email in background
            jobs.Enqueue<IncidentNotificationJob>(j => j.SendUpdatedAsync(incident.Id));

            return Results.NoContent();
        });

        // DELETE /api/incidents/{id} — Admin/SafetyManager only
        group.MapDelete("/{id:int}", async (int id, SafetyPortalDbContext db) =>
        {
            var incident = await db.IncidentReports.FindAsync(id);
            if (incident is null)
                return Results.NotFound();

            db.IncidentReports.Remove(incident);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization("SafetyManagerOrAdmin");

        return app;
    }
}
