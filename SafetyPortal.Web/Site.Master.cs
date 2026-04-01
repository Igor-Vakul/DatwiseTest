using System;
using System.Web.UI;

namespace SafetyPortal.Web
{
    public partial class SiteMaster : MasterPage
    {
        protected string UserFullName { get; private set; }
        protected string UserRole { get; private set; }
        protected bool IsAdmin { get; private set; }
        protected bool IsHebrew { get; private set; }
        protected string Dir { get; private set; }

        protected string LangUrlEn { get; private set; }
        protected string LangUrlHe { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn(Session))
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            UserFullName = SessionHelper.GetFullName(Session);
            UserRole = SessionHelper.GetRole(Session);
            IsAdmin = SessionHelper.IsAdmin(Session);
            IsHebrew = LanguageHelper.IsHebrew(Session);
            Dir = IsHebrew ? AppConstants.Layout.DirRtl : AppConstants.Layout.DirLtr;

            // Build language switch URLs preserving existing query string params
            var qs = System.Web.HttpUtility.ParseQueryString(Request.QueryString.ToString());
            qs.Remove("lang");
            var baseQs = qs.Count > 0 ? qs.ToString() + "&" : string.Empty;
            LangUrlEn = "?" + baseQs + "lang=en";
            LangUrlHe = "?" + baseQs + "lang=he";
        }

        /// <summary>Culture is already set by BasePage.InitializeCulture() on the same thread.</summary>
        protected string T(string key)
        {
            var val = System.Web.HttpContext.GetGlobalResourceObject("Strings", key) as string;
            return val ?? key;
        }

        protected string IsActive(string page)
        {
            var current = Request.AppRelativeCurrentExecutionFilePath
                               ?.TrimStart('~', '/') ?? string.Empty;
            return current.Equals(page, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
        }
    }
}
