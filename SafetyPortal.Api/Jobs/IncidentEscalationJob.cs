using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Services;
using static SafetyPortal.Api.AppConstants;

namespace SafetyPortal.Api.Jobs;

/// <summary>
/// Scenario 3 — Scheduled/delayed job.
/// Scheduled at incident creation to run AppConstants.Jobs.EscalationAfterDays later.
/// If the incident is still "Open" at that point, it notifies all admins and
/// safety managers so they can take action.
/// </summary>
public class IncidentEscalationJob
{
    private readonly SafetyPortalDbContext _db;
    private readonly IEmailService         _email;
    private readonly ILogger<IncidentEscalationJob> _logger;

    public IncidentEscalationJob(
        SafetyPortalDbContext               db,
        IEmailService                       email,
        ILogger<IncidentEscalationJob>      logger)
    {
        _db     = db;
        _email  = email;
        _logger = logger;
    }

    public async Task CheckAndEscalateAsync(int incidentId)
    {
        var incident = await _db.IncidentReports
            .Include(x => x.ReportedByUser)
            .FirstOrDefaultAsync(x => x.Id == incidentId);

        if (incident is null)
        {
            _logger.LogWarning("IncidentEscalationJob: incident {Id} not found", incidentId);
            return;
        }

        // Skip if already resolved — no escalation needed
        if (incident.Status != IncidentStatus.Open)
        {
            _logger.LogInformation(
                "IncidentEscalationJob: incident {ReportNumber} is '{Status}', skipping escalation",
                incident.ReportNumber, incident.Status);
            return;
        }

        var openDays = (int)(DateTime.UtcNow - incident.ReportedAt).TotalDays;

        var managers = await _db.Users
            .Where(u => u.IsActive &&
                        (u.RoleId == (int)AppConstants.Roles.Admin ||
                         u.RoleId == (int)AppConstants.Roles.SafetyManager))
            .Select(u => new { u.FullName, u.Email })
            .ToListAsync();

        if (managers.Count == 0)
        {
            _logger.LogWarning("IncidentEscalationJob: no admins/managers found to notify for incident {Id}", incidentId);
            return;
        }

        _logger.LogWarning(
            "IncidentEscalationJob: escalating incident {ReportNumber} ({OpenDays} day(s) open) to {ManagerCount} manager(s)",
            incident.ReportNumber, openDays, managers.Count);

        var ctx = new IncidentEscalationContext(
            ReportNumber:  incident.ReportNumber,
            Title:         incident.Title,
            SeverityLevel: incident.SeverityLevel,
            OpenDays:      openDays,
            ReporterName:  incident.ReportedByUser.FullName,
            ReporterEmail: incident.ReportedByUser.Email,
            Managers:      managers.Select(m => (m.FullName, m.Email)).ToList()
        );

        await _email.SendIncidentEscalationAsync(ctx);
    }
}
