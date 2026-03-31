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
        protected System.Web.UI.WebControls.TextBox  txtColor;
        protected System.Web.UI.WebControls.CheckBox chkActive;
        protected System.Web.UI.HiddenField          hfEditId;
        protected System.Web.UI.HiddenField          hfDeleteId;
        protected System.Web.UI.WebControls.Button   btnSave;
        protected System.Web.UI.WebControls.Button   btnDelete;

        protected List<DepartmentAdminItem> Departments { get; private set; } = new List<DepartmentAdminItem>();
        protected string Message     { get; private set; } = string.Empty;
        protected string MessageType { get; private set; } = "info";

        protected void Page_Load(object sender, EventArgs e)
        {
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
            var color    = txtColor.Text.Trim();
            var editId   = int.Parse(hfEditId.Value);

            if (string.IsNullOrEmpty(name)) { Message = "Name is required."; MessageType = "danger"; LoadDepartments(); return; }
            if (!System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$"))
                color = "#6c757d";

            bool ok = editId == 0
                ? Api.CreateDepartment(name, location, color)
                : Api.UpdateDepartment(editId, name, location, color, chkActive.Checked);

            Message     = ok ? "Saved successfully." : "Save failed.";
            MessageType = ok ? "success" : "danger";
            LoadDepartments();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteId.Value, out int id) || id == 0) return;
            bool ok     = Api.DeleteDepartment(id);
            Message     = ok ? "Department deleted." : "Cannot delete — department has existing incidents.";
            MessageType = ok ? "success" : "danger";
            LoadDepartments();
        }
    }
}
