using System.Web.UI;

namespace SafetyPortal.Web
{
    /// <summary>Base page — redirects to Login if not authenticated.</summary>
    public class BasePage : Page
    {
        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
            if (!SessionHelper.IsLoggedIn(Session))
            {
                Response.Redirect("~/Login.aspx?returnUrl=" +
                    System.Web.HttpUtility.UrlEncode(Request.RawUrl), true);
            }
        }

        protected string CurrentRole    => SessionHelper.GetRole(Session);
        protected string CurrentUser    => SessionHelper.GetFullName(Session);
        protected int    CurrentUserId  => SessionHelper.GetUserId(Session);
        protected bool   IsAdmin        => SessionHelper.IsAdmin(Session);
        protected bool   IsManagerOrAdmin => SessionHelper.IsManagerOrAdmin(Session);

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
