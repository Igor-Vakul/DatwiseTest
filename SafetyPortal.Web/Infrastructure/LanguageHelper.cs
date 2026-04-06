using System.Globalization;
using System.Threading;
using System.Web.SessionState;

namespace SafetyPortal.Web
{
    public static class LanguageHelper
    {
        public const string SessionKey = "lang";

        public static bool IsHebrew(HttpSessionState session)
            => session?[SessionKey] as string == "he";

        public static void SetLanguage(HttpSessionState session, string lang)
        {
            if (session != null && (lang == "he" || lang == "en"))
                session[SessionKey] = lang;
        }

        public static void ApplyCulture(HttpSessionState session)
        {
            var lang = session?[SessionKey] as string ?? "en";
            var culture = lang == "he"
                ? new CultureInfo("he-IL")
                : new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
