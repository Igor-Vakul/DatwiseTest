namespace SafetyPortal.Web
{
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

    public static class AppConstants
    {
        public static class Pagination
        {
            public const int DefaultPageSize = 20;
        }

        public static class Validation
        {
            public const int MinPasswordLength = 8;
            public const int DefaultActionDueDays = 14;
            public const string ISODateFormat = "yyyy-MM-dd";

            /// <summary>
            /// At least 8 chars, one uppercase, one lowercase, one digit, one special char.
            /// </summary>
            public const string PasswordRegex =
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;':"",./<>?]).{8,}$";
        }
    }
}
