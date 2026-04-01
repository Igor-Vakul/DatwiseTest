using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Admin
{
    public partial class UsersAdmin : AdminPage
    {
        // Create form
        protected System.Web.UI.WebControls.TextBox txtName;
        protected System.Web.UI.WebControls.TextBox txtEmail;
        protected System.Web.UI.WebControls.TextBox txtPassword;
        protected System.Web.UI.WebControls.DropDownList ddlRole;
        protected System.Web.UI.WebControls.Button btnCreate;

        // Edit form
        protected System.Web.UI.WebControls.HiddenField hdnEditId;
        protected System.Web.UI.WebControls.TextBox txtEditName;
        protected System.Web.UI.WebControls.TextBox txtEditPassword;
        protected System.Web.UI.WebControls.DropDownList ddlEditRole;
        protected System.Web.UI.WebControls.Button btnSaveEdit;

        // Send email form
        protected System.Web.UI.WebControls.HiddenField hdnEmailUserId;
        protected System.Web.UI.WebControls.HiddenField hdnEmailAddress;
        protected System.Web.UI.WebControls.TextBox txtEmailSubject;
        protected System.Web.UI.WebControls.TextBox txtEmailBody;
        protected System.Web.UI.WebControls.Button btnSendEmail;

        protected List<UserSummary> Users { get; private set; } = new List<UserSummary>();
        protected string Message { get; private set; } = string.Empty;
        protected string MessageType { get; private set; } = "info";
        protected string EmailError { get; private set; } = string.Empty;
        protected int EditingId { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnCreate.Text = T("create_user");
            btnSaveEdit.Text = T("save_user");
            btnSendEmail.Text = T("send_btn");

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

                // Handle edit GET — pre-fill edit modal
                var editId = Request.QueryString["edit"];
                if (!string.IsNullOrEmpty(editId) && int.TryParse(editId, out int eid))
                {
                    EditingId = eid;
                    hdnEditId.Value = eid.ToString();
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

            ddlEditRole.Items.Clear();
            foreach (var r in roles)
                ddlEditRole.Items.Add(new ListItem(r.Name, r.Id.ToString()));

            // Pre-select role in edit form if editing
            if (!string.IsNullOrEmpty(hdnEditId.Value) && int.TryParse(hdnEditId.Value, out int eid))
            {
                var users = Api.GetAllUsers() ?? new List<UserSummary>();
                var target = users.Find(u => u.Id == eid);
                if (target != null)
                {
                    txtEditName.Text = target.FullName;
                    var item = ddlEditRole.Items.FindByText(target.RoleName);
                    if (item != null) item.Selected = true;
                }
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            var name = StripHtml(txtName.Text.Trim());
            var email = StripHtml(txtEmail.Text.Trim());
            var pass = txtPassword.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                Message = T("fields_required");
                MessageType = "danger";
                LoadUsers();
                LoadRoles();
                return;
            }

            var passError = ValidatePassword(pass);
            if (passError != null)
            {
                Message = T(passError);
                MessageType = "danger";
                LoadUsers();
                LoadRoles();
                return;
            }

            var req = new CreateUserRequest
            {
                FullName = name,
                Email = email,
                Password = pass,
                RoleId = int.Parse(ddlRole.SelectedValue)
            };

            if (Api.CreateUser(req))
            {
                Message = T("user_created");
                MessageType = "success";
                txtName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                txtPassword.Text = string.Empty;
            }
            else
            {
                Message = T("user_create_fail");
                MessageType = "danger";
            }

            LoadUsers();
            LoadRoles();
        }

        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnEmailUserId.Value, out int uid) || uid == 0)
            {
                EmailError = T("email_send_fail");
                LoadUsers(); LoadRoles();
                return;
            }

            if (uid == CurrentUserId)
            {
                EmailError = T("email_self_send");
                LoadUsers(); LoadRoles();
                return;
            }

            var subject = StripHtml(txtEmailSubject.Text.Trim());
            var body = StripHtml(txtEmailBody.Text.Trim());

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                EmailError = T("email_subject_required");
                LoadUsers(); LoadRoles();
                return;
            }

            if (Api.SendEmailToUser(uid, subject, body))
            {
                Message = T("email_sent");
                MessageType = "success";
                hdnEmailUserId.Value = string.Empty;
                hdnEmailAddress.Value = string.Empty;
                txtEmailSubject.Text = string.Empty;
                txtEmailBody.Text = string.Empty;
            }
            else
            {
                EmailError = T("email_send_fail");
            }

            LoadUsers(); LoadRoles();
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hdnEditId.Value, out int uid) || uid == 0)
            {
                Message = T("user_update_fail");
                MessageType = "danger";
                LoadUsers();
                LoadRoles();
                return;
            }

            var name = StripHtml(txtEditName.Text.Trim());
            if (string.IsNullOrEmpty(name))
            {
                Message = T("fields_required");
                MessageType = "danger";
                LoadUsers();
                LoadRoles();
                return;
            }

            var pass = txtEditPassword.Text;
            if (!string.IsNullOrEmpty(pass))
            {
                var passError = ValidatePassword(pass);
                if (passError != null)
                {
                    Message = T(passError);
                    MessageType = "danger";
                    LoadUsers();
                    LoadRoles();
                    return;
                }
            }

            var req = new UpdateUserRequest
            {
                FullName = name,
                RoleId = int.Parse(ddlEditRole.SelectedValue),
                Password = string.IsNullOrEmpty(pass) ? null : pass
            };

            if (Api.UpdateUser(uid, req))
            {
                Message = T("user_updated");
                MessageType = "success";
                hdnEditId.Value = string.Empty;
            }
            else
            {
                Message = T("user_update_fail");
                MessageType = "danger";
            }

            LoadUsers();
            LoadRoles();
        }
    }
}
