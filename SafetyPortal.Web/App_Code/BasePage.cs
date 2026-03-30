using System.Web.UI;

namespace SafetyPortal.Web
{
    /// <summary>Base page — sets culture, redirects to Login if not authenticated.</summary>
    public class BasePage : Page
    {
        protected override void InitializeCulture()
        {
            LanguageHelper.ApplyCulture(Session);
            base.InitializeCulture();
        }

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);

            // Handle language switch via ?lang=
            var langParam = Request.QueryString["lang"];
            if (langParam == "he" || langParam == "en")
            {
                LanguageHelper.SetLanguage(Session, langParam);
                var url = Request.RawUrl;
                var token = "lang=" + langParam;
                var idx = url.IndexOf(token);
                if (idx >= 0)
                {
                    url = url.Remove(idx, token.Length)
                             .Replace("&&", "&")
                             .TrimEnd('&', '?');
                    var qi = url.IndexOf('?');
                    if (qi >= 0 && url.Length == qi + 1)
                        url = url.Substring(0, qi);
                }
                Response.Redirect(url.Length > 0 ? url : "~/Dashboard.aspx", true);
                return;
            }

            if (!SessionHelper.IsLoggedIn(Session))
            {
                Response.Redirect("~/Login.aspx?returnUrl=" +
                    System.Web.HttpUtility.UrlEncode(Request.RawUrl), true);
            }
        }

        protected string CurrentRole      => SessionHelper.GetRole(Session);
        protected string CurrentUser      => SessionHelper.GetFullName(Session);
        protected int    CurrentUserId    => SessionHelper.GetUserId(Session);
        protected bool   IsAdmin          => SessionHelper.IsAdmin(Session);
        protected bool   IsManagerOrAdmin => SessionHelper.IsManagerOrAdmin(Session);
        protected bool   IsHebrew         => LanguageHelper.IsHebrew(Session);
        protected string Dir              => IsHebrew ? "rtl" : "ltr";

        /// <summary>Looks up a string from App_GlobalResources/Strings.resx (culture-aware).</summary>
        protected string T(string key)
        {
            var val = GetGlobalResourceObject("Strings", key) as string;
            return val ?? key;
        }

        protected ApiClient Api => new ApiClient(SessionHelper.GetToken(Session));
    }

    /// <summary>Admin-only base page.</summary>
    public class AdminPage : BasePage
    {
        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
            if (!IsAdmin)
                Response.Redirect("~/Dashboard.aspx", true);
        }
    }
}
