using System;
using System.Web.UI;

namespace SafetyPortal.Web
{
    public partial class Login : Page
    {
        protected string ErrorMessage { get; private set; } = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Logout action
            if (Request.QueryString["action"] == "logout")
            {
                SessionHelper.Clear(Session);
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            // Already logged in
            if (SessionHelper.IsLoggedIn(Session))
            {
                Response.Redirect("~/Dashboard.aspx", true);
                return;
            }

            if (!IsPostBack)
                hdnReturnUrl.Value = Request.QueryString["returnUrl"] ?? "~/Dashboard.aspx";
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var email    = txtEmail.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Please enter email and password.";
                return;
            }

            var api  = new ApiClient();
            var resp = api.Login(email, password);

            if (resp == null)
            {
                ErrorMessage = "Invalid email or password.";
                return;
            }

            SessionHelper.SetUser(Session, resp);

            var returnUrl = hdnReturnUrl.Value;
            if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith("/"))
                returnUrl = "~/Dashboard.aspx";

            Response.Redirect(returnUrl, true);
        }
    }
}
