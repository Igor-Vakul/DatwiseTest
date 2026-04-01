using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using static SafetyPortal.Api.AppConstants;

namespace SafetyPortal.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard/stats", async (ClaimsPrincipal user, SafetyPortalDbContext db) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var isEmployee = user.IsInRole(RoleName.Employee.ToString());
            int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);

            var active = isEmployee
                ? db.IncidentReports.Where(x => !x.IsArchived && (x.ReportedByUserId == currentUserId || x.AssignedToUserId == currentUserId))
                : db.IncidentReports.Where(x => !x.IsArchived);

            var totalIncidents = await active.CountAsync();
            var openIncidents = await active.CountAsync(x => x.Status == IncidentStatus.Open.ToString() || x.Status == IncidentStatus.InProgress.ToString());
            var closedIncidents = await active.CountAsync(x => x.Status == IncidentStatus.Closed.ToString());
            var highCritical = await active.CountAsync(x =>
                                        (x.Status == IncidentStatus.Open.ToString() || x.Status == IncidentStatus.InProgress.ToString()) &&
                                        (x.SeverityLevel == SeverityLevel.High.ToString() || x.SeverityLevel == SeverityLevel.Critical.ToString()));

            var overdueActions = await db.CorrectiveActions.CountAsync(x =>
                                        x.Status != ActionStatus.Completed.ToString() && x.DueDate < today);
            var pendingActions = await db.CorrectiveActions.CountAsync(x => x.Status == ActionStatus.Pending.ToString());

            var byCategory = await active
                .Include(x => x.Category)
                .GroupBy(x => x.Category.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToListAsync();

            var byDepartment = await active
                .Include(x => x.Department)
                .GroupBy(x => new { x.Department.Name, x.Department.Color })
                .Select(g => new { DepartmentName = g.Key.Name, Color = g.Key.Color, Count = g.Count() })
                .ToListAsync();

            var bySeverity = await active
                .GroupBy(x => x.SeverityLevel)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();

            var byStatus = await active
                .GroupBy(x => x.Status)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-(AppConstants.Dashboard.TrendMonthsLookback - 1));
            // Materialize year/month keys first — string interpolation with :D2 cannot be translated to SQL
            var byMonthRaw = await active
                .Where(x => x.IncidentDate >= sixMonthsAgo)
                .GroupBy(x => new { x.IncidentDate.Year, x.IncidentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var byMonth = byMonthRaw
                .Select(x => new { Month = $"{x.Year}-{x.Month:D2}", Count = x.Count })
                .ToList();

            var recentIncidents = await active
                .Include(x => x.Attachments)
                .OrderByDescending(x => x.ReportedAt)
                .Take(AppConstants.Dashboard.RecentIncidentsCount)
                .Select(x => new
                {
                    x.Id,
                    x.ReportNumber,
                    x.Title,
                    x.SeverityLevel,
                    x.Status,
                    x.IncidentDate,
                    AttachmentsCount = x.Attachments.Count
                })
                .ToListAsync();

            return Results.Ok(new
            {
                TotalIncidents = totalIncidents,
                OpenIncidents = openIncidents,
                ClosedIncidents = closedIncidents,
                HighCriticalIncidents = highCritical,
                OverdueActions = overdueActions,
                PendingActions = pendingActions,
                ByCategory = byCategory,
                ByDepartment = byDepartment,
                BySeverity = bySeverity,
                ByStatus = byStatus,
                ByMonth = byMonth,
                RecentIncidents = recentIncidents
            });
        })
        .WithTags("Dashboard")
        .RequireAuthorization();

        return app;
    }
}
