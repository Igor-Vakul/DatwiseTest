namespace SafetyPortal.Web
{
    public enum RoleName     { Admin, SafetyManager, Supervisor, Employee }
    public enum IncidentStatus { Open, InProgress, Closed }
    public enum SeverityLevel  { Low, Medium, High, Critical }
    public enum ActionStatus   { Pending, InProgress, Completed }

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
