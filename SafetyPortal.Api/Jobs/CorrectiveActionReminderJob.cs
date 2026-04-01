using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Services;
using static SafetyPortal.Api.AppConstants;

namespace SafetyPortal.Api.Jobs;

/// <summary>
/// Scenario 2 — Recurring job (daily at 08:00).
/// Finds every non-completed corrective action whose due date is exactly
/// AppConstants.Jobs.ReminderDaysBeforeDue days away and sends a reminder
/// email to the assignee.
/// </summary>
public class CorrectiveActionReminderJob
{
    private readonly SafetyPortalDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<CorrectiveActionReminderJob> _logger;

    public CorrectiveActionReminderJob(
        SafetyPortalDbContext db,
        IEmailService email,
        ILogger<CorrectiveActionReminderJob> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task SendRemindersAsync()
    {
        var targetDate = DateOnly.FromDateTime(
            DateTime.Today.AddDays(AppConstants.Jobs.ReminderDaysBeforeDue));

        var actions = await _db.CorrectiveActions
            .Include(ca => ca.AssignedToUser)
            .Include(ca => ca.Report)
            .Where(ca => ca.Status != ActionStatus.Completed.ToString()
                      && ca.CompletedAt == null
                      && ca.DueDate == targetDate)
            .ToListAsync();

        if (actions.Count == 0)
        {
            _logger.LogInformation("CorrectiveActionReminderJob: no reminders to send for {Date}", targetDate);
            return;
        }

        _logger.LogInformation("CorrectiveActionReminderJob: sending {Count} reminder(s) for due date {Date}",
            actions.Count, targetDate);

        var tasks = actions.Select(ca =>
        {
            var ctx = new CorrectiveActionReminderContext(
                ActionId: ca.Id,
                ActionTitle: ca.ActionTitle,
                ReportNumber: ca.Report.ReportNumber,
                DueDate: ca.DueDate,
                DaysLeft: AppConstants.Jobs.ReminderDaysBeforeDue,
                AssigneeName: ca.AssignedToUser.FullName,
                AssigneeEmail: ca.AssignedToUser.Email
            );
            return _email.SendCorrectiveActionReminderAsync(ctx);
        });

        await Task.WhenAll(tasks);
    }
}
