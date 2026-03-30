using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Incidents
{
    public partial class IncidentDetails : BasePage
    {
        protected System.Web.UI.WebControls.TextBox       txtActionTitle;
        protected System.Web.UI.WebControls.TextBox       txtActionDesc;
        protected System.Web.UI.WebControls.DropDownList  ddlActionUser;
        protected System.Web.UI.WebControls.TextBox       txtActionDue;
        protected System.Web.UI.WebControls.DropDownList  ddlActionPriority;
        protected System.Web.UI.WebControls.Button        btnAddAction;

        protected IncidentDetail Incident    { get; private set; }
        protected string         ActionError { get; private set; } = string.Empty;

        private int IncidentId => int.TryParse(Request.QueryString["id"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (btnAddAction != null) btnAddAction.Text = T("save_action");

            // Handle "complete action" GET action
            if (!IsPostBack)
            {
                var completeActionId = Request.QueryString["completeAction"];
                if (!string.IsNullOrEmpty(completeActionId) && int.TryParse(completeActionId, out int caId))
                {
                    Api.UpdateActionStatus(caId, "Completed");
                    Response.Redirect($"Details.aspx?id={IncidentId}", true);
                    return;
                }
            }

            Incident = Api.GetIncident(IncidentId);

            if (!IsPostBack && Incident != null && IsManagerOrAdmin)
            {
                LoadUsers();
                txtActionDue.Text = DateTime.Today.AddDays(AppConstants.Validation.DefaultActionDueDays).ToString("yyyy-MM-dd");
            }
        }

        private void LoadUsers()
        {
            var users = Api.GetUsers() ?? new List<UserLookupItem>();
            ddlActionUser.Items.Clear();
            foreach (var u in users)
                ddlActionUser.Items.Add(new ListItem($"{u.FullName} [{u.RoleName}]", u.Id.ToString()));
        }

        protected void btnAddAction_Click(object sender, EventArgs e)
        {
            Incident = Api.GetIncident(IncidentId);

            var title = txtActionTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                ActionError = "Action title is required.";
                LoadUsers();
                return;
            }

            if (!DateTime.TryParse(txtActionDue.Text, out var dueDate))
            {
                ActionError = "Invalid due date.";
                LoadUsers();
                return;
            }

            var req = new CreateCorrectiveActionRequest
            {
                ReportId          = IncidentId,
                ActionTitle       = title,
                ActionDescription = txtActionDesc.Text.Trim(),
                AssignedToUserId  = int.Parse(ddlActionUser.SelectedValue),
                DueDate           = dueDate.ToString("yyyy-MM-dd"),
                PriorityLevel     = ddlActionPriority.SelectedValue
            };

            if (Api.CreateCorrectiveAction(req))
                Response.Redirect($"Details.aspx?id={IncidentId}", true);
            else
            {
                ActionError = "Failed to add action.";
                LoadUsers();
            }
        }
    }
}
