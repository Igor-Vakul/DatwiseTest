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
                ? db.IncidentReports.Where(x => !x.IsArchived && (
                    x.ReportedByUserId == currentUserId ||
                    x.AssignedToUserId == currentUserId ||
                    x.CorrectiveActions.Any(ca => ca.AssignedToUserId == currentUserId)))
                : db.IncidentReports.Where(x => !x.IsArchived);

            var firstPendingActionStatusId = await db.ActionStatusOptions
                .Where(s => s.IsActive && !s.IsCompleted)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            var totalIncidents = await active.CountAsync();
            var openIncidents  = await active.CountAsync(x => !x.StatusOption.IsClosing);
            var closedIncidents = await active.CountAsync(x => x.StatusOption.IsClosing);
            var highCritical = await active.CountAsync(x =>
                !x.StatusOption.IsClosing &&
                (x.SeverityLevelOption.Name == SeverityLevel.High.ToString() ||
                 x.SeverityLevelOption.Name == SeverityLevel.Critical.ToString()));

            var overdueActions = await db.CorrectiveActions.CountAsync(x =>
                !x.StatusOption.IsCompleted && x.DueDate < today);
            var pendingActions = await db.CorrectiveActions.CountAsync(x =>
                x.StatusId == firstPendingActionStatusId);

            var byCategory = await active
                .GroupBy(x => x.Category.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToListAsync();

            var byDepartment = await active
                .GroupBy(x => new { x.Department.Name, x.Department.Color })
                .Select(g => new { DepartmentName = g.Key.Name, Color = g.Key.Color, Count = g.Count() })
                .ToListAsync();

            var bySeverity = await active
                .GroupBy(x => x.SeverityLevelOption.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();

            var byStatus = await active
                .GroupBy(x => x.StatusOption.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-(AppConstants.Dashboard.TrendMonthsLookback - 1));
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
                .Include(x => x.StatusOption)
                .Include(x => x.SeverityLevelOption)
                .Include(x => x.Attachments)
                .OrderByDescending(x => x.ReportedAt)
                .Take(AppConstants.Dashboard.RecentIncidentsCount)
                .Select(x => new
                {
                    x.Id,
                    x.ReportNumber,
                    x.Title,
                    SeverityLevel = x.SeverityLevelOption.Name,
                    Status = x.StatusOption.Name,
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
