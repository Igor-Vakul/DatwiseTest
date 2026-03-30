using System;
using System.Web.UI;

namespace SafetyPortal.Web
{
    public partial class SiteMaster : MasterPage
    {
        protected string UserFullName { get; private set; }
        protected string UserRole     { get; private set; }
        protected bool   IsAdmin      { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn(Session))
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            UserFullName = SessionHelper.GetFullName(Session);
            UserRole     = SessionHelper.GetRole(Session);
            IsAdmin      = SessionHelper.IsAdmin(Session);
        }

        protected string IsActive(string page)
        {
            var current = Request.AppRelativeCurrentExecutionFilePath
                               ?.TrimStart('~', '/') ?? string.Empty;
            return current.Equals(page, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
        }
    }
}
