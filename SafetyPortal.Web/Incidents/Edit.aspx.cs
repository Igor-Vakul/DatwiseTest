using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SafetyPortal.Shared;
using SafetyPortal.Shared.Models;
using SafetyPortal.Web.Services;

namespace SafetyPortal.Web.Incidents
{
    public partial class IncidentEdit : BasePage
    {
        protected System.Web.UI.WebControls.TextBox txtTitle;
        protected System.Web.UI.WebControls.TextBox txtDescription;
        protected System.Web.UI.WebControls.DropDownList ddlCategory;
        protected System.Web.UI.WebControls.DropDownList ddlDept;
        protected System.Web.UI.WebControls.TextBox txtDate;
        protected System.Web.UI.WebControls.DropDownList ddlSeverity;
        protected System.Web.UI.WebControls.DropDownList ddlStatus;
        protected System.Web.UI.WebControls.TextBox txtLocation;
        protected System.Web.UI.WebControls.DropDownList ddlAssign;
        protected System.Web.UI.WebControls.Button btnSave;

        protected string ErrorMessage { get; private set; } = string.Empty;
        protected int IncidentId => int.TryParse(Request.QueryString["id"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            btnSave.Text = Translate("save_changes");
            if (!IsPostBack)
            {
                var incident = new IncidentService(Token).GetIncident(IncidentId);
                if (incident == null)
                {
                    Response.Redirect("~/Incidents/List.aspx", true);
                    return;
                }
                LoadLookups(incident);
                PopulateForm(incident);
            }
        }

        private void LoadLookups(IncidentDetail incident)
        {
            var lookup = new LookupService(Token);
            foreach (var c in lookup.GetCategories() ?? new List<CategoryItem>())
                ddlCategory.Items.Add(new ListItem(c.Name, c.Id.ToString()));

            foreach (var d in lookup.GetDepartments() ?? new List<DepartmentItem>())
                ddlDept.Items.Add(new ListItem($"{d.Name} ({d.LocationName})", d.Id.ToString()));

            ddlAssign.Items.Add(new ListItem("— Unassigned —", ""));
            foreach (var u in lookup.GetUsers() ?? new List<UserLookupItem>())
                ddlAssign.Items.Add(new ListItem($"{u.FullName} [{u.RoleName}]", u.Id.ToString()));
        }

        private void PopulateForm(IncidentDetail inc)
        {
            txtTitle.Text = inc.Title;
            txtDescription.Text = inc.Description;
            txtDate.Text = inc.IncidentDate.ToString(AppConstants.Validation.ISODateFormat);
            txtLocation.Text = inc.LocationDetails ?? string.Empty;

            SetSelected(ddlCategory, inc.CategoryId.ToString());
            SetSelected(ddlDept, inc.DepartmentId.ToString());
            SetSelected(ddlSeverity, inc.SeverityLevel);
            SetSelected(ddlStatus, inc.Status);
            SetSelected(ddlAssign, inc.AssignedToUserId?.ToString() ?? "");
        }

        private static void SetSelected(DropDownList ddl, string val)
        {
            var item = ddl.Items.FindByValue(val);
            if (item != null) item.Selected = true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!DateTime.TryParse(txtDate.Text, out var date))
            {
                ErrorMessage = "Invalid date.";
                return;
            }

            int? assignedTo = int.TryParse(ddlAssign.SelectedValue, out int uid) ? uid : (int?)null;

            var req = new UpdateIncidentRequest
            {
                Title = StripHtml(txtTitle.Text.Trim()),
                Description = StripHtml(txtDescription.Text.Trim()),
                CategoryId = int.Parse(ddlCategory.SelectedValue),
                DepartmentId = int.Parse(ddlDept.SelectedValue),
                IncidentDate = date.ToString(AppConstants.Validation.ISODateFormat),
                LocationDetails = StripHtml(txtLocation.Text.Trim()),
                SeverityLevel = ddlSeverity.SelectedValue,
                Status = ddlStatus.SelectedValue,
                AssignedToUserId = assignedTo
            };

            if (new IncidentService(Token).UpdateIncident(IncidentId, req))
                Response.Redirect($"~/Incidents/Details.aspx?id={IncidentId}", true);
            else
                ErrorMessage = "Failed to update incident.";
        }
    }
}
