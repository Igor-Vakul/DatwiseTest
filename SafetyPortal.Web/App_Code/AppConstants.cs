namespace SafetyPortal.Web
{
    public static class AppConstants
    {
        public static class Pagination
        {
            public const int DefaultPageSize = 20;
        }

        public static class Validation
        {
            public const int    MinPasswordLength    = 6;
            public const int    DefaultActionDueDays = 14;
            public const string ISODateFormat        = "yyyy-MM-dd";
        }
    }
}
