using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Incidents
{
    public partial class IncidentEdit : BasePage
    {
        protected string ErrorMessage { get; private set; } = string.Empty;
        protected int    IncidentId   => int.TryParse(Request.QueryString["id"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var incident = Api.GetIncident(IncidentId);
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
            foreach (var c in Api.GetCategories() ?? new List<CategoryItem>())
                ddlCategory.Items.Add(new ListItem(c.Name, c.Id.ToString()));

            foreach (var d in Api.GetDepartments() ?? new List<DepartmentItem>())
                ddlDept.Items.Add(new ListItem($"{d.Name} ({d.LocationName})", d.Id.ToString()));

            ddlAssign.Items.Add(new ListItem("— Unassigned —", ""));
            foreach (var u in Api.GetUsers() ?? new List<UserLookupItem>())
                ddlAssign.Items.Add(new ListItem($"{u.FullName} [{u.RoleName}]", u.Id.ToString()));
        }

        private void PopulateForm(IncidentDetail inc)
        {
            txtTitle.Text       = inc.Title;
            txtDescription.Text = inc.Description;
            txtDate.Text        = inc.IncidentDate.ToString("yyyy-MM-dd");
            txtLocation.Text    = inc.LocationDetails ?? string.Empty;

            SetSelected(ddlCategory, inc.CategoryId.ToString());
            SetSelected(ddlDept,     inc.DepartmentId.ToString());
            SetSelected(ddlSeverity, inc.SeverityLevel);
            SetSelected(ddlStatus,   inc.Status);
            SetSelected(ddlAssign,   inc.AssignedToUserId?.ToString() ?? "");
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
                Title           = txtTitle.Text.Trim(),
                Description     = txtDescription.Text.Trim(),
                CategoryId      = int.Parse(ddlCategory.SelectedValue),
                DepartmentId    = int.Parse(ddlDept.SelectedValue),
                IncidentDate    = date,
                LocationDetails = txtLocation.Text.Trim(),
                SeverityLevel   = ddlSeverity.SelectedValue,
                Status          = ddlStatus.SelectedValue,
                AssignedToUserId = assignedTo
            };

            if (Api.UpdateIncident(IncidentId, req))
                Response.Redirect($"~/Incidents/Details.aspx?id={IncidentId}", true);
            else
                ErrorMessage = "Failed to update incident.";
        }
    }
}
