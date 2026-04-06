using SafetyPortal.Shared.Models;
using SafetyPortal.Web.Services;
using System;
using System.Collections.Generic;

namespace SafetyPortal.Web.Admin
{
    public partial class DepartmentsAdmin : AdminPage
    {
        protected System.Web.UI.WebControls.TextBox txtName;
        protected System.Web.UI.WebControls.TextBox txtLocation;
        protected System.Web.UI.WebControls.HiddenField hfColor;
        protected System.Web.UI.WebControls.HiddenField hfActive;
        protected System.Web.UI.WebControls.HiddenField hfEditId;
        protected System.Web.UI.WebControls.HiddenField hfDeleteId;
        protected System.Web.UI.WebControls.Button btnSave;
        protected System.Web.UI.WebControls.Button btnDelete;

        protected List<DepartmentAdminItem> Departments { get; private set; } = new List<DepartmentAdminItem>();
        protected string Message { get; private set; } = string.Empty;
        protected string MessageType { get; private set; } = "info";

        protected void Page_Load(object sender, EventArgs e)
        {
            var toggleId = Request.QueryString["toggle"];
            if (!string.IsNullOrEmpty(toggleId) && int.TryParse(toggleId, out int tid))
            {
                new AdminService(Token).ToggleDepartmentActive(tid);
                Response.Redirect("Departments.aspx", true);
                return;
            }

            txtLocation.Attributes["list"] = "locationsList";
            if (!IsPostBack)
                LoadDepartments();
        }

        private void LoadDepartments()
        {
            Departments = new AdminService(Token).GetAllDepartments() ?? new List<DepartmentAdminItem>();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var name = StripHtml(txtName.Text.Trim());
            var location = StripHtml(txtLocation.Text.Trim());
            var color = hfColor.Value.Trim();
            var editId = int.Parse(hfEditId.Value);

            if (string.IsNullOrEmpty(name)) { Message = "Name is required."; MessageType = "danger"; LoadDepartments(); return; }
            if (!System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$"))
                color = "#6c757d";

            bool isActive = string.Equals(hfActive.Value, "true", StringComparison.OrdinalIgnoreCase);
            var adminSvc = new AdminService(Token);
            bool ok = editId == 0
                ? adminSvc.CreateDepartment(name, location, color)
                : adminSvc.UpdateDepartment(editId, name, location, color, isActive);

            Message = ok ? Translate("dept_saved") : Translate("dept_save_fail");
            MessageType = ok ? "success" : "danger";
            LoadDepartments();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteId.Value, out int id) || id == 0) return;
            bool ok = new AdminService(Token).DeleteDepartment(id);
            Message = ok ? Translate("dept_deleted") : Translate("dept_delete_fail");
            MessageType = ok ? "success" : "danger";
            LoadDepartments();
        }
    }
}
