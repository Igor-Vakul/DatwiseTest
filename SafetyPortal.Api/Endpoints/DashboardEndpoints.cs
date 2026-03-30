using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;

namespace SafetyPortal.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/stats", async (SafetyPortalDbContext db) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var totalIncidents    = await db.IncidentReports.CountAsync();
            var openIncidents     = await db.IncidentReports.CountAsync(x => x.Status == "Open" || x.Status == "InProgress");
            var closedIncidents   = await db.IncidentReports.CountAsync(x => x.Status == "Closed");
            var highCritical      = await db.IncidentReports.CountAsync(x =>
                                        (x.Status == "Open" || x.Status == "InProgress") &&
                                        (x.SeverityLevel == "High" || x.SeverityLevel == "Critical"));

            var overdueActions    = await db.CorrectiveActions.CountAsync(x =>
                                        x.Status != "Completed" && x.DueDate < today);
            var pendingActions    = await db.CorrectiveActions.CountAsync(x => x.Status == "Pending");

            var byCategory = await db.IncidentReports
                .Include(x => x.Category)
                .GroupBy(x => x.Category.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToListAsync();

            var byDepartment = await db.IncidentReports
                .Include(x => x.Department)
                .GroupBy(x => x.Department.Name)
                .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                .ToListAsync();

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-(AppConstants.Dashboard.TrendMonthsLookback - 1));
            // Materialize year/month keys first — string interpolation with :D2 cannot be translated to SQL
            var byMonthRaw = await db.IncidentReports
                .Where(x => x.IncidentDate >= sixMonthsAgo)
                .GroupBy(x => new { x.IncidentDate.Year, x.IncidentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var byMonth = byMonthRaw
                .Select(x => new { Month = $"{x.Year}-{x.Month:D2}", Count = x.Count })
                .ToList();

            var recentIncidents = await db.IncidentReports
                .OrderByDescending(x => x.ReportedAt)
                .Take(AppConstants.Dashboard.RecentIncidentsCount)
                .Select(x => new
                {
                    x.Id,
                    x.ReportNumber,
                    x.Title,
                    x.SeverityLevel,
                    x.Status,
                    x.IncidentDate
                })
                .ToListAsync();

            return Results.Ok(new
            {
                TotalIncidents    = totalIncidents,
                OpenIncidents     = openIncidents,
                ClosedIncidents   = closedIncidents,
                HighCriticalIncidents = highCritical,
                OverdueActions    = overdueActions,
                PendingActions    = pendingActions,
                ByCategory        = byCategory,
                ByDepartment      = byDepartment,
                ByMonth           = byMonth,
                RecentIncidents   = recentIncidents
            });
        })
        .WithTags("Dashboard")
        .RequireAuthorization();

        return app;
    }
}
