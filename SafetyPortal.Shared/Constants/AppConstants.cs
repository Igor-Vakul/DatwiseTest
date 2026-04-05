namespace SafetyPortal.Shared
{
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

            public const string PasswordRegex =
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;':"",./<>?]).{8,}$";
        }
    }
}
