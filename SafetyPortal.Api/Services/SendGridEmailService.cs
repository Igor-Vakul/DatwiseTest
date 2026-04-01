using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SafetyPortal.Api.Services;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        IOptions<SendGridOptions> options,
        ILogger<SendGridEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    // ── Scenario 1 ─────────────────────────────────────────────────────────

    public async Task SendIncidentCreatedAsync(IncidentEmailContext ctx)
    {
        var tasks = new List<Task>
        {
            Send(ctx.ReporterEmail, ctx.ReporterName,
                 $"[SafetyPortal] Incident {ctx.ReportNumber} submitted",
                 BuildCreatedHtml(ctx, isAssignee: false))
        };

        if (!string.IsNullOrEmpty(ctx.AssigneeEmail) &&
            !ctx.AssigneeEmail.Equals(ctx.ReporterEmail, StringComparison.OrdinalIgnoreCase))
        {
            tasks.Add(Send(ctx.AssigneeEmail, ctx.AssigneeName ?? ctx.AssigneeEmail,
                           $"[SafetyPortal] You have been assigned to incident {ctx.ReportNumber}",
                           BuildCreatedHtml(ctx, isAssignee: true)));
        }

        await Task.WhenAll(tasks);
    }

    public async Task SendIncidentUpdatedAsync(IncidentEmailContext ctx)
    {
        var tasks = new List<Task>
        {
            Send(ctx.ReporterEmail, ctx.ReporterName,
                 $"[SafetyPortal] Incident {ctx.ReportNumber} updated",
                 BuildUpdatedHtml(ctx, isAssignee: false))
        };

        if (!string.IsNullOrEmpty(ctx.AssigneeEmail) &&
            !ctx.AssigneeEmail.Equals(ctx.ReporterEmail, StringComparison.OrdinalIgnoreCase))
        {
            tasks.Add(Send(ctx.AssigneeEmail, ctx.AssigneeName ?? ctx.AssigneeEmail,
                           $"[SafetyPortal] Incident {ctx.ReportNumber} assigned to you has been updated",
                           BuildUpdatedHtml(ctx, isAssignee: true)));
        }

        await Task.WhenAll(tasks);
    }

    // ── Scenario 2: corrective action reminder ─────────────────────────────

    public Task SendCorrectiveActionReminderAsync(CorrectiveActionReminderContext ctx)
        => Send(
            ctx.AssigneeEmail,
            ctx.AssigneeName,
            $"[SafetyPortal] Reminder: corrective action due in {ctx.DaysLeft} day(s)",
            BuildReminderHtml(ctx));

    // ── Scenario 3: incident escalation ───────────────────────────────────

    public Task SendDirectAsync(string toEmail, string toName, string subject, string body)
    {
        var html = Wrap("#5b6abf", subject,
            $"Hello {System.Net.WebUtility.HtmlEncode(toName)},",
            $"<p style='white-space:pre-wrap;'>{System.Net.WebUtility.HtmlEncode(body)}</p>");
        return Send(toEmail, toName, subject, html);
    }

    public Task SendIncidentEscalationAsync(IncidentEscalationContext ctx)
    {
        var tasks = ctx.Managers
            .Select(m => Send(m.Email, m.Name,
                              $"[SafetyPortal] ESCALATION: incident {ctx.ReportNumber} open for {ctx.OpenDays} day(s)",
                              BuildEscalationHtml(ctx, m.Name)))
            .ToList();

        return Task.WhenAll(tasks);
    }

    // ── Core send ──────────────────────────────────────────────────────────

    private async Task Send(string to, string toName, string subject, string html)
    {
        try
        {
            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var message = MailHelper.CreateSingleEmail(
                from, new EmailAddress(to, toName), subject,
                plainTextContent: null, htmlContent: html);

            var response = await client.SendEmailAsync(message);

            if ((int)response.StatusCode >= 400)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogWarning("SendGrid {Status} for {To}: {Body}", response.StatusCode, to, body);
            }
            else
            {
                _logger.LogInformation("Email sent → {To}: {Subject}", to, subject);
            }
        }
        catch (Exception ex)
        {
            // Email failure must never break the main flow
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }

    // ── HTML builders ──────────────────────────────────────────────────────

    private static string BuildCreatedHtml(IncidentEmailContext ctx, bool isAssignee)
    {
        var greeting = isAssignee
            ? $"Hello {ctx.AssigneeName},<br>You have been assigned to a new incident report."
            : $"Hello {ctx.ReporterName},<br>Your incident report has been successfully submitted.";

        return Wrap("#d9534f", "New Incident Report", greeting, IncidentTable(ctx));
    }

    private static string BuildUpdatedHtml(IncidentEmailContext ctx, bool isAssignee)
    {
        var greeting = isAssignee
            ? $"Hello {ctx.AssigneeName},<br>An incident assigned to you has been updated."
            : $"Hello {ctx.ReporterName},<br>Your incident report has been updated.";

        return Wrap("#f0ad4e", "Incident Report Updated", greeting, IncidentTable(ctx));
    }

    private static string BuildReminderHtml(CorrectiveActionReminderContext ctx) =>
        Wrap("#5bc0de", "Corrective Action Reminder",
             $"Hello {ctx.AssigneeName},<br>A corrective action assigned to you is due in <strong>{ctx.DaysLeft} day(s)</strong>.",
             $"""
             <table style="border-collapse:collapse;width:100%;">
               <tr><td style="padding:6px;font-weight:bold;width:160px;">Action</td>
                   <td style="padding:6px;">{ctx.ActionTitle}</td></tr>
               <tr style="background:#f5f5f5;">
                   <td style="padding:6px;font-weight:bold;">Incident</td>
                   <td style="padding:6px;">{ctx.ReportNumber}</td></tr>
               <tr><td style="padding:6px;font-weight:bold;">Due Date</td>
                   <td style="padding:6px;">{ctx.DueDate:yyyy-MM-dd}</td></tr>
               <tr style="background:#f5f5f5;">
                   <td style="padding:6px;font-weight:bold;">Days Remaining</td>
                   <td style="padding:6px;color:#d9534f;font-weight:bold;">{ctx.DaysLeft}</td></tr>
             </table>
             """);

    private static string BuildEscalationHtml(IncidentEscalationContext ctx, string managerName) =>
        Wrap("#d9534f", "&#9888; Incident Escalation Alert",
             $"Hello {managerName},<br>The following incident has been <strong>Open for {ctx.OpenDays} day(s)</strong> without resolution and requires your attention.",
             $"""
             <table style="border-collapse:collapse;width:100%;">
               <tr><td style="padding:6px;font-weight:bold;width:160px;">Report #</td>
                   <td style="padding:6px;">{ctx.ReportNumber}</td></tr>
               <tr style="background:#f5f5f5;">
                   <td style="padding:6px;font-weight:bold;">Title</td>
                   <td style="padding:6px;">{ctx.Title}</td></tr>
               <tr><td style="padding:6px;font-weight:bold;">Severity</td>
                   <td style="padding:6px;">{ctx.SeverityLevel}</td></tr>
               <tr style="background:#f5f5f5;">
                   <td style="padding:6px;font-weight:bold;">Reported By</td>
                   <td style="padding:6px;">{ctx.ReporterName}</td></tr>
               <tr><td style="padding:6px;font-weight:bold;">Days Open</td>
                   <td style="padding:6px;color:#d9534f;font-weight:bold;">{ctx.OpenDays}</td></tr>
             </table>
             """);

    private static string IncidentTable(IncidentEmailContext ctx) => $"""
        <table style="border-collapse:collapse;width:100%;">
          <tr><td style="padding:6px;font-weight:bold;width:140px;">Report #</td>
              <td style="padding:6px;">{ctx.ReportNumber}</td></tr>
          <tr style="background:#f5f5f5;">
              <td style="padding:6px;font-weight:bold;">Title</td>
              <td style="padding:6px;">{ctx.Title}</td></tr>
          <tr><td style="padding:6px;font-weight:bold;">Severity</td>
              <td style="padding:6px;">{ctx.SeverityLevel}</td></tr>
          <tr style="background:#f5f5f5;">
              <td style="padding:6px;font-weight:bold;">Status</td>
              <td style="padding:6px;">{ctx.Status}</td></tr>
          <tr><td style="padding:6px;font-weight:bold;">Reporter</td>
              <td style="padding:6px;">{ctx.ReporterName}</td></tr>
          <tr style="background:#f5f5f5;">
              <td style="padding:6px;font-weight:bold;">Assigned To</td>
              <td style="padding:6px;">{ctx.AssigneeName ?? "—"}</td></tr>
        </table>
        """;

    private static string Wrap(string headerColor, string heading, string intro, string body) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;">
          <h2 style="color:{headerColor};">{heading}</h2>
          <p>{intro}</p>
          {body}
          <hr/>
          <p style="font-size:12px;color:#888;">This is an automated message from SafetyPortal. Do not reply.</p>
        </div>
        """;
}
