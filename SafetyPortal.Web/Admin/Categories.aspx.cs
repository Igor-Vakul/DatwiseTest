using SafetyPortal.Shared.Models;
using SafetyPortal.Web.Services;
using System;
using System.Collections.Generic;

namespace SafetyPortal.Web.Admin
{
    public partial class CategoriesAdmin : AdminPage
    {
        protected System.Web.UI.WebControls.TextBox txtName;
        protected System.Web.UI.WebControls.TextBox txtDescription;
        protected System.Web.UI.WebControls.HiddenField hfActive;
        protected System.Web.UI.WebControls.HiddenField hfEditId;
        protected System.Web.UI.WebControls.HiddenField hfDeleteId;
        protected System.Web.UI.WebControls.Button btnSave;
        protected System.Web.UI.WebControls.Button btnDelete;

        protected List<CategoryAdminItem> Categories { get; private set; } = new List<CategoryAdminItem>();
        protected string Message { get; private set; } = string.Empty;
        protected string MessageType { get; private set; } = "info";

        protected void Page_Load(object sender, EventArgs e)
        {
            var toggleId = Request.QueryString["toggle"];
            if (!string.IsNullOrEmpty(toggleId) && int.TryParse(toggleId, out int tid))
            {
                new AdminService(Token).ToggleCategoryActive(tid);
                Response.Redirect("Categories.aspx", true);
                return;
            }

            if (!IsPostBack)
                LoadCategories();
        }

        private void LoadCategories()
        {
            Categories = new AdminService(Token).GetAllCategories() ?? new List<CategoryAdminItem>();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var name = StripHtml(txtName.Text.Trim());
            var desc = StripHtml(txtDescription.Text.Trim());
            var editId = int.Parse(hfEditId.Value);

            if (string.IsNullOrEmpty(name)) { Message = Translate("fields_required"); MessageType = "danger"; LoadCategories(); return; }

            bool isActive = string.Equals(hfActive.Value, "true", StringComparison.OrdinalIgnoreCase);
            var adminSvc = new AdminService(Token);
            bool ok = editId == 0
                ? adminSvc.CreateCategory(name, desc)
                : adminSvc.UpdateCategory(editId, name, desc, isActive);

            Message = ok ? Translate("cat_saved") : Translate("cat_save_fail");
            MessageType = ok ? "success" : "danger";
            LoadCategories();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteId.Value, out int id) || id == 0) return;
            bool ok = new AdminService(Token).DeleteCategory(id);
            Message = ok ? Translate("cat_deleted") : Translate("cat_delete_fail");
            MessageType = ok ? "success" : "danger";
            LoadCategories();
        }
    }
}
