using System.Security.Claims;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using static SafetyPortal.Api.AppConstants;
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
            [AsParameters] IncidentFilterQuery f,
            ClaimsPrincipal user,
            SafetyPortalDbContext db) =>
        {
            var isEmployee = user.IsInRole(RoleName.Employee.ToString());
            int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);

            var query = db.IncidentReports
                .Include(x => x.Category)
                .Include(x => x.Department)
                .Include(x => x.ReportedByUser)
                .Include(x => x.StatusOption)
                .Include(x => x.SeverityLevelOption)
                .Include(x => x.CorrectiveActions)
                .Include(x => x.Attachments)
                .Where(x => x.IsArchived == f.Archived)
                .AsQueryable();

            if (isEmployee)
                query = query.Where(x =>
                    x.ReportedByUserId == currentUserId ||
                    x.AssignedToUserId == currentUserId ||
                    x.CorrectiveActions.Any(ca => ca.AssignedToUserId == currentUserId));

            if (!string.IsNullOrWhiteSpace(f.Search))
                query = query.Where(x =>
                    x.Title.Contains(f.Search) ||
                    x.ReportNumber.Contains(f.Search) ||
                    x.Description.Contains(f.Search));

            if (!string.IsNullOrWhiteSpace(f.Status))
                query = query.Where(x => x.StatusOption.Name == f.Status);

            if (!string.IsNullOrWhiteSpace(f.SeverityLevel))
                query = query.Where(x => x.SeverityLevelOption.Name == f.SeverityLevel);

            if (f.DepartmentId.HasValue)
                query = query.Where(x => x.DepartmentId == f.DepartmentId);

            if (f.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == f.CategoryId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.ReportedAt)
                .Skip((f.Page - 1) * f.PageSize)
                .Take(f.PageSize)
                .Select(x => new IncidentSummaryDto(
                    x.Id,
                    x.ReportNumber,
                    x.Title,
                    x.Category.Name,
                    x.Department.Name,
                    x.ReportedByUser.FullName,
                    x.IncidentDate,
                    x.SeverityLevelOption.Name,
                    x.StatusOption.Name,
                    x.CorrectiveActions.Count,
                    x.Attachments.Count,
                    x.IsArchived
                ))
                .ToListAsync();

            return Results.Ok(new { Items = items, TotalCount = totalCount, Page = f.Page, PageSize = f.PageSize });
        });

        // GET /api/incidents/export — filtered Excel download
        group.MapGet("/export", async (
            [AsParameters] IncidentFilterQuery f,
            ClaimsPrincipal user,
            SafetyPortalDbContext db) =>
        {
            var isEmployee = user.IsInRole(RoleName.Employee.ToString());
            int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);

            var query = db.IncidentReports
                .Include(x => x.Category)
                .Include(x => x.Department)
                .Include(x => x.ReportedByUser)
                .Include(x => x.AssignedToUser)
                .Include(x => x.StatusOption)
                .Include(x => x.SeverityLevelOption)
                .Include(x => x.CorrectiveActions)
                .Where(x => x.IsArchived == f.Archived)
                .AsQueryable();

            if (isEmployee)
                query = query.Where(x =>
                    x.ReportedByUserId == currentUserId ||
                    x.AssignedToUserId == currentUserId ||
                    x.CorrectiveActions.Any(ca => ca.AssignedToUserId == currentUserId));

            if (!string.IsNullOrWhiteSpace(f.Search))
                query = query.Where(x =>
                    x.Title.Contains(f.Search) ||
                    x.ReportNumber.Contains(f.Search) ||
                    x.Description.Contains(f.Search));

            if (!string.IsNullOrWhiteSpace(f.Status))
                query = query.Where(x => x.StatusOption.Name == f.Status);

            if (!string.IsNullOrWhiteSpace(f.SeverityLevel))
                query = query.Where(x => x.SeverityLevelOption.Name == f.SeverityLevel);

            if (f.DepartmentId.HasValue)
                query = query.Where(x => x.DepartmentId == f.DepartmentId);

            if (f.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == f.CategoryId);

            var rows = await query
                .OrderByDescending(x => x.ReportedAt)
                .Select(x => new IncidentExportRow
                {
                    ReportNumber = x.ReportNumber,
                    Title = x.Title,
                    CategoryName = x.Category.Name,
                    DepartmentName = x.Department.Name,
                    ReportedBy = x.ReportedByUser.FullName,
                    AssignedTo = x.AssignedToUser != null ? x.AssignedToUser.FullName : "",
                    IncidentDate = x.IncidentDate,
                    SeverityLevel = x.SeverityLevelOption.Name,
                    Status = x.StatusOption.Name,
                    Location = x.LocationDetails ?? "",
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
                .Include(x => x.StatusOption)
                .Include(x => x.SeverityLevelOption)
                .Include(x => x.CorrectiveActions)
                    .ThenInclude(ca => ca.AssignedToUser)
                .Include(x => x.CorrectiveActions)
                    .ThenInclude(ca => ca.StatusOption)
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
                incident.SeverityLevelOption.Name,
                incident.StatusOption.Name,
                incident.IsArchived,
                incident.CorrectiveActions.Select(ca => new CorrectiveActionSummaryDto(
                    ca.Id,
                    ca.ReportId,
                    incident.ReportNumber,
                    ca.ActionTitle,
                    ca.AssignedToUser!.FullName,
                    ca.DueDate,
                    ca.StatusOption.Name,
                    ca.PriorityLevel
                )).ToList()
            );

            return Results.Ok(dto);
        });

        // POST /api/incidents
        group.MapPost("/", async (
            CreateIncidentDto request,
            ClaimsPrincipal user,
            SafetyPortalDbContext db,
            IBackgroundJobClient jobs) =>
        {
            if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Results.Unauthorized();

            var severityOption = await db.SeverityLevelOptions
                .FirstOrDefaultAsync(s => s.Name == request.SeverityLevel && s.IsActive);
            if (severityOption is null)
                return Results.BadRequest(new { error = "Invalid severity level." });

            var defaultStatus = await db.IncidentStatusOptions
                .Where(s => s.IsActive && !s.IsClosing)
                .OrderBy(s => s.DisplayOrder)
                .FirstOrDefaultAsync();
            if (defaultStatus is null)
                return Results.StatusCode(500);

            var count = await db.IncidentReports.CountAsync();
            var reportNumber = $"INC-{DateTime.UtcNow:yyyy}-{(count + 1):D4}";

            var incident = new IncidentReport
            {
                ReportNumber = reportNumber,
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                DepartmentId = request.DepartmentId,
                ReportedByUserId = userId,
                AssignedToUserId = request.AssignedToUserId,
                IncidentDate = DateTime.Parse(request.IncidentDate),
                ReportedAt = DateTime.UtcNow,
                LocationDetails = request.LocationDetails,
                SeverityLevelId = severityOption.Id,
                StatusId = defaultStatus.Id
            };

            db.IncidentReports.Add(incident);
            await db.SaveChangesAsync();

            jobs.Enqueue<IncidentNotificationJob>(j => j.SendCreatedAsync(incident.Id));
            jobs.Schedule<IncidentEscalationJob>(
                j => j.CheckAndEscalateAsync(incident.Id),
                TimeSpan.FromDays(AppConstants.Jobs.EscalationAfterDays));

            return Results.Created($"/api/incidents/{incident.Id}", new { incident.Id, incident.ReportNumber });
        });

        // PUT /api/incidents/{id}
        group.MapPut("/{id:int}", async (
            int id,
            UpdateIncidentDto request,
            SafetyPortalDbContext db,
            IBackgroundJobClient jobs) =>
        {
            var incident = await db.IncidentReports
                .Include(x => x.ReportedByUser)
                .Include(x => x.CorrectiveActions)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (incident is null)
                return Results.NotFound();

            var severityOption = await db.SeverityLevelOptions
                .FirstOrDefaultAsync(s => s.Name == request.SeverityLevel && s.IsActive);
            if (severityOption is null)
                return Results.BadRequest(new { error = "Invalid severity level." });

            var newStatusOption = await db.IncidentStatusOptions
                .FirstOrDefaultAsync(s => s.Name == request.Status);
            if (newStatusOption is null)
                return Results.BadRequest(new { error = "Invalid status." });

            if (newStatusOption.IsClosing && incident.StatusId != newStatusOption.Id)
            {
                var completedStatus = await db.ActionStatusOptions
                    .Where(s => s.IsCompleted)
                    .FirstOrDefaultAsync();

                if (completedStatus is not null)
                    foreach (var ca in incident.CorrectiveActions)
                        ca.StatusId = completedStatus.Id;
            }

            incident.Title = request.Title;
            incident.Description = request.Description;
            incident.CategoryId = request.CategoryId;
            incident.DepartmentId = request.DepartmentId;
            incident.IncidentDate = DateTime.Parse(request.IncidentDate);
            incident.LocationDetails = request.LocationDetails;
            incident.SeverityLevelId = severityOption.Id;
            incident.AssignedToUserId = request.AssignedToUserId;
            incident.StatusId = newStatusOption.Id;

            await db.SaveChangesAsync();

            jobs.Enqueue<IncidentNotificationJob>(j => j.SendUpdatedAsync(incident.Id));

            return Results.NoContent();
        });

        // PUT /api/incidents/{id}/archive — toggle archive flag
        group.MapPut("/{id:int}/archive", async (int id, SafetyPortalDbContext db) =>
        {
            var incident = await db.IncidentReports
                .Include(x => x.StatusOption)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (incident is null)
                return Results.NotFound();

            if (!incident.IsArchived && !incident.StatusOption.IsClosing)
                return Results.BadRequest(new { error = "Only closed incidents can be archived." });

            incident.IsArchived = !incident.IsArchived;
            await db.SaveChangesAsync();
            return Results.Ok(new { incident.Id, incident.IsArchived });
        })
        .RequireAuthorization("SafetyManagerOrAdmin");

        // DELETE /api/incidents/{id}
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
