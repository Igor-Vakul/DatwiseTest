namespace SafetyPortal.Api;

public static class AppConstants
{
    public static class Pagination
    {
        public const int DefaultPage     = 1;
        public const int DefaultPageSize = 20;
    }

    public static class Dashboard
    {
        public const int RecentIncidentsCount  = 5;
        public const int TrendMonthsLookback   = 6;
    }

    public static class Roles
    {
        public const int AdminId         = 1;
        public const int SafetyManagerId = 2;
    }

    public static class Jobs
    {
        /// <summary>Send reminder N days before corrective action due date.</summary>
        public const int    ReminderDaysBeforeDue = 3;

        /// <summary>Escalate an incident that stays Open for this many days.</summary>
        public const int    EscalationAfterDays   = 3;

        /// <summary>Cron: every day at 08:00 server time.</summary>
        public const string ReminderCron          = "0 8 * * *";
    }
}
