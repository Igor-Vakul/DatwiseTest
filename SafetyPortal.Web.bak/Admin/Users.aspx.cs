using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Admin
{
    public partial class UsersAdmin : AdminPage
    {
        protected List<UserSummary> Users       { get; private set; } = new List<UserSummary>();
        protected string            Message     { get; private set; } = string.Empty;
        protected string            MessageType { get; private set; } = "info";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Handle toggle-active GET
            if (!IsPostBack)
            {
                var toggleId = Request.QueryString["toggle"];
                if (!string.IsNullOrEmpty(toggleId) && int.TryParse(toggleId, out int tid))
                {
                    Api.ToggleUserActive(tid);
                    Response.Redirect("Users.aspx", true);
                    return;
                }

                LoadRoles();
            }

            LoadUsers();
        }

        private void LoadUsers()
        {
            Users = Api.GetAllUsers() ?? new List<UserSummary>();
        }

        private void LoadRoles()
        {
            var roles = Api.GetRoles() ?? new List<RoleItem>();
            ddlRole.Items.Clear();
            foreach (var r in roles)
                ddlRole.Items.Add(new ListItem(r.Name, r.Id.ToString()));
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            var name  = txtName.Text.Trim();
            var email = txtEmail.Text.Trim();
            var pass  = txtPassword.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                Message     = "All fields are required.";
                MessageType = "danger";
                LoadUsers();
                LoadRoles();
                return;
            }

            if (pass.Length < 6)
            {
                Message     = "Password must be at least 6 characters.";
                MessageType = "danger";
                LoadUsers();
                LoadRoles();
                return;
            }

            var req = new CreateUserRequest
            {
                FullName = name,
                Email    = email,
                Password = pass,
                RoleId   = int.Parse(ddlRole.SelectedValue)
            };

            if (Api.CreateUser(req))
            {
                Message         = $"User '{name}' created successfully.";
                MessageType     = "success";
                txtName.Text    = string.Empty;
                txtEmail.Text   = string.Empty;
                txtPassword.Text = string.Empty;
            }
            else
            {
                Message     = "Failed to create user (email may already exist).";
                MessageType = "danger";
            }

            LoadUsers();
            LoadRoles();
        }
    }
}
