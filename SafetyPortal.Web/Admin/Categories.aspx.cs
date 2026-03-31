using System;
using System.Collections.Generic;
using System.Web.UI;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Admin
{
    public partial class CategoriesAdmin : AdminPage
    {
        protected System.Web.UI.WebControls.TextBox  txtName;
        protected System.Web.UI.WebControls.TextBox  txtDescription;
        protected System.Web.UI.WebControls.CheckBox chkActive;
        protected System.Web.UI.WebControls.HiddenField hfEditId;
        protected System.Web.UI.WebControls.HiddenField hfDeleteId;
        protected System.Web.UI.WebControls.Button   btnSave;
        protected System.Web.UI.WebControls.Button   btnDelete;

        protected List<CategoryAdminItem> Categories { get; private set; } = new List<CategoryAdminItem>();
        protected string Message     { get; private set; } = string.Empty;
        protected string MessageType { get; private set; } = "info";

        protected void Page_Load(object sender, EventArgs e)
        {
            var toggleId = Request.QueryString["toggle"];
            if (!string.IsNullOrEmpty(toggleId) && int.TryParse(toggleId, out int tid))
            {
                Api.ToggleCategoryActive(tid);
                Response.Redirect("Categories.aspx", true);
                return;
            }

            if (!IsPostBack)
                LoadCategories();
        }

        private void LoadCategories()
        {
            Categories = Api.GetAllCategories() ?? new List<CategoryAdminItem>();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var name   = StripHtml(txtName.Text.Trim());
            var desc   = StripHtml(txtDescription.Text.Trim());
            var editId = int.Parse(hfEditId.Value);

            if (string.IsNullOrEmpty(name)) { Message = "Name is required."; MessageType = "danger"; LoadCategories(); return; }

            bool ok = editId == 0
                ? Api.CreateCategory(name, desc)
                : Api.UpdateCategory(editId, name, desc, chkActive.Checked);

            Message     = ok ? "Saved successfully." : "Save failed.";
            MessageType = ok ? "success" : "danger";
            LoadCategories();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteId.Value, out int id) || id == 0) return;
            bool ok     = Api.DeleteCategory(id);
            Message     = ok ? "Category deleted." : "Cannot delete — category has open incidents.";
            MessageType = ok ? "success" : "danger";
            LoadCategories();
        }
    }
}
