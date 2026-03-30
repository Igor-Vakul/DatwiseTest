using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Incidents
{
    public partial class IncidentCreate : BasePage
    {
        protected System.Web.UI.WebControls.TextBox       txtTitle;
        protected System.Web.UI.WebControls.TextBox       txtDescription;
        protected System.Web.UI.WebControls.DropDownList  ddlCategory;
        protected System.Web.UI.WebControls.DropDownList  ddlDept;
        protected System.Web.UI.WebControls.TextBox       txtDate;
        protected System.Web.UI.WebControls.DropDownList  ddlSeverity;
        protected System.Web.UI.WebControls.TextBox       txtLocation;
        protected System.Web.UI.WebControls.DropDownList  ddlAssign;
        protected System.Web.UI.WebControls.Button        btnSave;

        protected string ErrorMessage { get; private set; } = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            btnSave.Text = T("submit_report");
            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Today.ToString(AppConstants.Validation.ISODateFormat);
                LoadLookups();
            }
        }

        private void LoadLookups()
        {
            var cats  = Api.GetCategories()  ?? new List<CategoryItem>();
            var depts = Api.GetDepartments() ?? new List<DepartmentItem>();
            var users = Api.GetUsers()       ?? new List<UserLookupItem>();

            foreach (var c in cats)
                ddlCategory.Items.Add(new ListItem(c.Name, c.Id.ToString()));

            foreach (var d in depts)
                ddlDept.Items.Add(new ListItem($"{d.Name} ({d.LocationName})", d.Id.ToString()));

            ddlAssign.Items.Add(new ListItem("— Unassigned —", ""));
            foreach (var u in users)
                ddlAssign.Items.Add(new ListItem($"{u.FullName} [{u.RoleName}]", u.Id.ToString()));
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var title = txtTitle.Text.Trim();
            var desc  = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            {
                ErrorMessage = "Title and Description are required.";
                return;
            }

            if (!DateTime.TryParse(txtDate.Text, out var date))
            {
                ErrorMessage = "Invalid incident date.";
                return;
            }

            int? assignedTo = int.TryParse(ddlAssign.SelectedValue, out int uid) ? uid : (int?)null;

            var req = new CreateIncidentRequest
            {
                Title           = title,
                Description     = desc,
                CategoryId      = int.Parse(ddlCategory.SelectedValue),
                DepartmentId    = int.Parse(ddlDept.SelectedValue),
                IncidentDate    = date.ToString(AppConstants.Validation.ISODateFormat),
                LocationDetails = txtLocation.Text.Trim(),
                SeverityLevel   = ddlSeverity.SelectedValue,
                AssignedToUserId = assignedTo
            };

            if (Api.CreateIncident(req))
                Response.Redirect("~/Incidents/List.aspx?created=1", true);
            else
                ErrorMessage = "Failed to submit report. Please try again.";
        }
    }
}
