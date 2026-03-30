using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Services;

namespace SafetyPortal.Api.Jobs;

/// <summary>
/// Scenario 1 — Fire-and-forget.
/// Sends email notifications when an incident is created or updated.
/// Enqueued by IncidentEndpoints immediately after SaveChangesAsync.
/// </summary>
public class IncidentNotificationJob
{
    private readonly SafetyPortalDbContext _db;
    private readonly IEmailService         _email;
    private readonly ILogger<IncidentNotificationJob> _logger;

    public IncidentNotificationJob(
        SafetyPortalDbContext             db,
        IEmailService                     email,
        ILogger<IncidentNotificationJob>  logger)
    {
        _db    = db;
        _email = email;
        _logger = logger;
    }

    public async Task SendCreatedAsync(int incidentId)
    {
        var incident = await _db.IncidentReports
            .Include(x => x.ReportedByUser)
            .Include(x => x.AssignedToUser)
            .FirstOrDefaultAsync(x => x.Id == incidentId);

        if (incident is null)
        {
            _logger.LogWarning("IncidentNotificationJob.SendCreatedAsync: incident {Id} not found", incidentId);
            return;
        }

        var ctx = new IncidentEmailContext(
            ReportNumber:  incident.ReportNumber,
            Title:         incident.Title,
            SeverityLevel: incident.SeverityLevel,
            Status:        incident.Status,
            ReporterName:  incident.ReportedByUser.FullName,
            ReporterEmail: incident.ReportedByUser.Email,
            AssigneeName:  incident.AssignedToUser?.FullName,
            AssigneeEmail: incident.AssignedToUser?.Email
        );

        await _email.SendIncidentCreatedAsync(ctx);
    }

    public async Task SendUpdatedAsync(int incidentId)
    {
        var incident = await _db.IncidentReports
            .Include(x => x.ReportedByUser)
            .Include(x => x.AssignedToUser)
            .FirstOrDefaultAsync(x => x.Id == incidentId);

        if (incident is null)
        {
            _logger.LogWarning("IncidentNotificationJob.SendUpdatedAsync: incident {Id} not found", incidentId);
            return;
        }

        var ctx = new IncidentEmailContext(
            ReportNumber:  incident.ReportNumber,
            Title:         incident.Title,
            SeverityLevel: incident.SeverityLevel,
            Status:        incident.Status,
            ReporterName:  incident.ReportedByUser.FullName,
            ReporterEmail: incident.ReportedByUser.Email,
            AssigneeName:  incident.AssignedToUser?.FullName,
            AssigneeEmail: incident.AssignedToUser?.Email
        );

        await _email.SendIncidentUpdatedAsync(ctx);
    }
}
