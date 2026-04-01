using System;
using System.Collections.Generic;
using System.Web.UI;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Admin
{
    public partial class DepartmentsAdmin : AdminPage
    {
        protected System.Web.UI.WebControls.TextBox  txtName;
        protected System.Web.UI.WebControls.TextBox  txtLocation;
        protected System.Web.UI.WebControls.HiddenField hfColor;
        protected System.Web.UI.WebControls.HiddenField hfActive;
        protected System.Web.UI.WebControls.HiddenField hfEditId;
        protected System.Web.UI.WebControls.HiddenField hfDeleteId;
        protected System.Web.UI.WebControls.Button   btnSave;
        protected System.Web.UI.WebControls.Button   btnDelete;

        protected List<DepartmentAdminItem> Departments { get; private set; } = new List<DepartmentAdminItem>();
        protected string Message     { get; private set; } = string.Empty;
        protected string MessageType { get; private set; } = "info";

        protected void Page_Load(object sender, EventArgs e)
        {
            var toggleId = Request.QueryString["toggle"];
            if (!string.IsNullOrEmpty(toggleId) && int.TryParse(toggleId, out int tid))
            {
                Api.ToggleDepartmentActive(tid);
                Response.Redirect("Departments.aspx", true);
                return;
            }

            txtLocation.Attributes["list"] = "locationsList";
            if (!IsPostBack)
                LoadDepartments();
        }

        private void LoadDepartments()
        {
            Departments = Api.GetAllDepartments() ?? new List<DepartmentAdminItem>();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var name     = StripHtml(txtName.Text.Trim());
            var location = StripHtml(txtLocation.Text.Trim());
            var color    = hfColor.Value.Trim();
            var editId   = int.Parse(hfEditId.Value);

            if (string.IsNullOrEmpty(name)) { Message = "Name is required."; MessageType = "danger"; LoadDepartments(); return; }
            if (!System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$"))
                color = "#6c757d";

            bool isActive = string.Equals(hfActive.Value, "true", StringComparison.OrdinalIgnoreCase);
            bool ok = editId == 0
                ? Api.CreateDepartment(name, location, color)
                : Api.UpdateDepartment(editId, name, location, color, isActive);

            Message     = ok ? "Saved successfully." : "Save failed.";
            MessageType = ok ? "success" : "danger";
            LoadDepartments();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteId.Value, out int id) || id == 0) return;
            bool ok     = Api.DeleteDepartment(id);
            Message     = ok ? "Department deleted." : "Cannot delete — department has open incidents.";
            MessageType = ok ? "success" : "danger";
            LoadDepartments();
        }
    }
}
