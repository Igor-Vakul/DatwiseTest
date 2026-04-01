namespace SafetyPortal.Api.Services;

public interface IEmailService
{
    // ── Scenario 1: incident created / updated ─────────────────────────────
    Task SendIncidentCreatedAsync(IncidentEmailContext ctx);
    Task SendIncidentUpdatedAsync(IncidentEmailContext ctx);

    // ── Scenario 2: corrective action due-date reminder ────────────────────
    Task SendCorrectiveActionReminderAsync(CorrectiveActionReminderContext ctx);

    // ── Scenario 3: incident escalation ───────────────────────────────────
    Task SendIncidentEscalationAsync(IncidentEscalationContext ctx);

    // ── Ad-hoc: direct message to a user ──────────────────────────────────
    Task SendDirectAsync(string toEmail, string toName, string subject, string body);
}

// ── Context records ────────────────────────────────────────────────────────

public record IncidentEmailContext(
    string ReportNumber,
    string Title,
    string SeverityLevel,
    string Status,
    string ReporterName,
    string ReporterEmail,
    string? AssigneeName,
    string? AssigneeEmail
);

public record CorrectiveActionReminderContext(
    int ActionId,
    string ActionTitle,
    string ReportNumber,
    DateOnly DueDate,
    int DaysLeft,
    string AssigneeName,
    string AssigneeEmail
);

public record IncidentEscalationContext(
    string ReportNumber,
    string Title,
    string SeverityLevel,
    int OpenDays,
    string ReporterName,
    string ReporterEmail,
    // All admins/safety managers to notify
    IReadOnlyList<(string Name, string Email)> Managers
);
