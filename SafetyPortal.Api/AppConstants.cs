namespace SafetyPortal.Api;

public static class AppConstants
{
    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 20;
    }

    public static class Dashboard
    {
        public const int RecentIncidentsCount = 5;
        public const int TrendMonthsLookback = 6;
    }

    public enum Roles
    {
        Admin = 1,
        SafetyManager = 2,
        Supervisor = 3,
        Employee = 4,
    }

    public enum RoleName
    {
        Admin = 1,
        SafetyManager = 2,
        Supervisor = 3,
        Employee = 4,
    }

    public enum IncidentStatus
    {
        Open = 1,
        InProgress = 2,
        Closed = 3,
    }

    public enum SeverityLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4,
    }

    public enum ActionStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
    }

    public static class Attachments
    {
        public const long MaxImageBytes = 5 * 1024 * 1024;  // 5 MB
        public const long MaxDocumentBytes = 20 * 1024 * 1024;  // 20 MB
        public const int SignatureBytesLen = 16;
        public const string StorageFolder = "attachments";
    }

    public static class Jobs
    {
        /// <summary>Send reminder N days before corrective action due date.</summary>
        public const int ReminderDaysBeforeDue = 3;

        /// <summary>Escalate an incident that stays Open for this many days.</summary>
        public const int EscalationAfterDays = 3;

        /// <summary>Cron: every day at 08:00 server time.</summary>
        public const string ReminderCron = "0 8 * * *";
    }
}
