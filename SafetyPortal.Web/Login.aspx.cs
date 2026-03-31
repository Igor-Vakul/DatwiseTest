using System;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace SafetyPortal.Web
{
    public partial class Login : Page
    {
        protected System.Web.UI.WebControls.TextBox       txtEmail;
        protected System.Web.UI.WebControls.TextBox       txtPassword;
        protected System.Web.UI.WebControls.HiddenField   hdnReturnUrl;
        protected System.Web.UI.WebControls.Button        btnLogin;

        protected string ErrorMessage { get; private set; } = string.Empty;
        protected bool   IsHebrew     => LanguageHelper.IsHebrew(Session);
        protected string Dir          => IsHebrew ? "rtl" : "ltr";

        protected override void InitializeCulture()
        {
            LanguageHelper.ApplyCulture(Session);
            base.InitializeCulture();
        }

        protected string T(string key)
        {
            var val = GetGlobalResourceObject("Strings", key) as string;
            return val ?? key;
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, @"<[^>]*>", string.Empty, RegexOptions.Singleline);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Handle ?lang= switch
            var langParam = Request.QueryString["lang"];
            if (langParam == "he" || langParam == "en")
            {
                LanguageHelper.SetLanguage(Session, langParam);
                Response.Redirect("~/Login.aspx", true);
                return;
            }

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

            btnLogin.Text = T("sign_in");

            if (!IsPostBack)
                hdnReturnUrl.Value = Request.QueryString["returnUrl"] ?? "~/Dashboard.aspx";
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var email    = StripHtml(txtEmail.Text.Trim());
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = T("enter_credentials");
                return;
            }

            var api  = new ApiClient();
            var resp = api.Login(email, password);

            if (resp == null)
            {
                ErrorMessage = T("invalid_login");
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
